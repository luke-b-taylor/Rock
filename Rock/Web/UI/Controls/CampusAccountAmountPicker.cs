using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    [ToolboxData( "<Rock:CampusAccountAmountPicker runat=\"server\" />" )]
    public class CampusAccountAmountPicker : CompositeControl
    {
        #region Controls

        #region Controls for SingleAccount Mode

        private Panel _pnlAccountAmountEntrySingle;
        private NumberBox _nbAmountAccountSingle;
        private ButtonDropDownList _ddlSingleAccountCampus;
        private ButtonDropDownList _ddlAccountSingle;

        #endregion Controls for SingleAccount Mode

        #region Controls for MultiAccount Mode

        private Panel _pnlAccountAmountEntryMulti;
        private Repeater _rptPromptForAccountAmountsMulti;
        private ButtonDropDownList _ddlMultiAccountCampus;

        #endregion Controls for MultiAccount Mode

        #endregion Controls

        #region Enums

        public enum AccountAmountEntryMode
        {
            SingleAccount,
            MultipleAccounts
        }

        #endregion Enums

        #region private constants

        private static class RepeaterControlIds
        {
            internal const string hfAccountAmountMultiAccountId = "hfAccountAmountMultiAccountId";
            internal const string nbAccountAmountMulti = "nbAccountAmountMulti";
        }

        #endregion private constants

        #region Properties

        /// <summary>
        /// Gets or sets the amount entry mode (Defaults to <seealso cref="AccountAmountEntryMode.Single"/> )
        /// </summary>
        /// <value>
        /// The amount entry mode.
        /// </value>
        public AccountAmountEntryMode AmountEntryMode
        {
            get => ViewState["AmountEntryMode"] as CampusAccountAmountPicker.AccountAmountEntryMode? ?? CampusAccountAmountPicker.AccountAmountEntryMode.SingleAccount;
            set => ViewState["AmountEntryMode"] = value;
        }

        /// <summary>
        /// Gets or sets the accountIds of the selectable accounts. This will be the accounts that will displayed.
        /// Note: This has special logic. The account(s) that the user selects <seealso cref="SelectedAccountIds"/> will be determined as follows:
        ///   1) If the selected account is not associated with a campus, the Selected Account will be the first matching child account that is associated with the selected campus.
        ///   2) If the selected account is not associated with a campus, but there are no child accounts for the selected campus, the parent account (the one the user sees) will be returned.
        ///   3) If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)
        /// </summary>
        /// <value>
        /// The selectable account ids.
        /// </value>
        public int[] SelectableAccountIds
        {
            get => ViewState["SelectableAccountIds"] as int[] ?? new int[0];
            set => ViewState["SelectableAccountIds"] = value;
        }

        /// <summary>
        /// Gets or sets the selected account ids.
        /// Note: This has special logic. See comments on <seealso cref="SelectableAccountIds"/>
        /// </summary>
        /// <value>
        /// The selected account ids.
        /// </value>
        public int[] SelectedAccountIds { get; set; }

        /// <summary>
        /// Set CampusId to set the default Campus that should be used. If this is set, <seealso cref="AskForCampusIfKnown"/> can optionally be set to false to hide the campus selector and to prevent changing the campus.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int? CampusId
        {
            get => ViewState["CampusId"] as int?;
            set => ViewState["CampusId"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Campus Prompt should be shown when <seealso cref="CampusId"/> is set.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [ask for campus if known]; otherwise, <c>false</c>.
        /// </value>
        public bool AskForCampusIfKnown
        {
            get => ViewState["AskForCampusIfKnown"] as bool? ?? true;
            set => ViewState["AskForCampusIfKnown"] = value;
        }

        /// <summary>
        /// Gets or CSS class that should be applied to the amount input when in SingleAccount mode
        /// </summary>
        /// <value>
        /// The amount entry single CSS class.
        /// </value>
        public string AmountEntrySingleCssClass
        {
            get
            {
                EnsureChildControls();
                return _pnlAccountAmountEntrySingle.CssClass;
            }

            set
            {
                EnsureChildControls();
                _pnlAccountAmountEntrySingle.CssClass = value;
            }
        }

        #endregion Properties

        #region private methods

        /// <summary>
        /// Loads the campuses.
        /// </summary>
        private void LoadCampuses()
        {
            _ddlSingleAccountCampus.Items.Clear();
            _ddlMultiAccountCampus.Items.Clear();
            foreach ( var campus in CampusCache.All().OrderBy( a => a.Order ) )
            {
                _ddlSingleAccountCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
                _ddlMultiAccountCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }

            CampusCache defaultCampus = CampusCache.All().FirstOrDefault();

            if ( this.CampusId.HasValue )
            {

                defaultCampus = CampusCache.Get( this.CampusId.Value );

            }

            if ( _ddlSingleAccountCampus.Items.Count > 0 )
            {
                _ddlSingleAccountCampus.SetValue( defaultCampus );
            }

            if ( _ddlMultiAccountCampus.Items.Count > 0 )
            {
                _ddlMultiAccountCampus.SetValue( defaultCampus );
            }
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            var rockContext = new RockContext();
            var selectableAccountIds = this.SelectableAccountIds.ToList();

            IQueryable<FinancialAccount> accountsQry;
            var financialAccountService = new FinancialAccountService( rockContext );

            if ( selectableAccountIds.Any() )
            {
                accountsQry = financialAccountService.GetByIds( selectableAccountIds );
            }
            else
            {
                accountsQry = financialAccountService.Queryable();
            }

            // limit to active, public accounts, and don't include ones that aren't within the date range
            accountsQry = accountsQry.Where( f =>
                    f.IsActive &&
                    f.IsPublic.HasValue &&
                    f.IsPublic.Value &&
                    ( f.StartDate == null || f.StartDate <= RockDateTime.Today ) &&
                    ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) )
                .OrderBy( f => f.Order );

            var accountsList = accountsQry.AsNoTracking().ToList();

            _ddlAccountSingle.Items.Clear();

            foreach ( var account in accountsList )
            {
                _ddlAccountSingle.Items.Add( new ListItem( account.PublicName, account.Id.ToString() ) );
            }

            _ddlAccountSingle.SetValue( accountsList.FirstOrDefault() );

            _rptPromptForAccountAmountsMulti.DataSource = accountsList;
            _rptPromptForAccountAmountsMulti.DataBind();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the selected amount, specify <paramref name="accountId"/> if <seealso cref="AmountEntryMode"/> = <seealso cref="AccountAmountEntryMode.MultipleAccounts"/>
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="accountId">The account identifier.</param>
        public void SetSelectedAmount( decimal? amount, int? accountId = null )
        {
            EnsureChildControls();

            // TODO
            //_nbAmountAccountSingle.Text = amount?.ToString();

            if ( AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
            {
                // TODO
            }
        }

        /// <summary>
        /// Gets the selected amount, specify <paramref name="accountId"/> if <seealso cref="AmountEntryMode"/> = <seealso cref="AccountAmountEntryMode.MultipleAccounts"/>
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public decimal? GetSelectedAmount( int? accountId = null )
        {
            EnsureChildControls();

            // TODO: Test

            if ( AmountEntryMode == AccountAmountEntryMode.MultipleAccounts && accountId.HasValue )
            {
                foreach ( var item in _rptPromptForAccountAmountsMulti.Items.OfType<RepeaterItem>() )
                {
                    var hfAccountAmountMultiAccountId = item.FindControl( RepeaterControlIds.hfAccountAmountMultiAccountId ) as HiddenField;
                    if ( hfAccountAmountMultiAccountId.Value.AsDecimal() == accountId.Value )
                    {
                        var nbAccountAmountMulti = item.FindControl( RepeaterControlIds.nbAccountAmountMulti ) as NumberBox;
                        return nbAccountAmountMulti.Text.AsDecimalOrNull();
                    }
                }
            }
            else
            {
                return _nbAmountAccountSingle.Text.AsDecimalOrNull();
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            LoadCampuses();
            BindAccounts();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            _pnlAccountAmountEntrySingle.Visible = AmountEntryMode == AccountAmountEntryMode.SingleAccount;
            _pnlAccountAmountEntryMulti.Visible = AmountEntryMode == AccountAmountEntryMode.MultipleAccounts;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            /* Single Account Mode */

            _pnlAccountAmountEntrySingle = new Panel() { CssClass = "account-amount-single-entry" };
            _pnlAccountAmountEntrySingle.ID = "_pnlAccountAmountEntrySingle";

            Controls.Add( _pnlAccountAmountEntrySingle );

            // Special big entry for entering a single dollar amount
            _nbAmountAccountSingle = new NumberBox();
            _nbAmountAccountSingle.ID = "_nbAmountAccountSingle";
            _nbAmountAccountSingle.Placeholder = "Enter Amount";
            _nbAmountAccountSingle.NumberType = ValidationDataType.Currency;
            _nbAmountAccountSingle.CssClass = "amount-input";
            _nbAmountAccountSingle.MinimumValue = "0";
            _pnlAccountAmountEntrySingle.Controls.Add( _nbAmountAccountSingle );

            var pnlSingleCampusDiv = new Panel() { CssClass = "" };
            _pnlAccountAmountEntrySingle.Controls.Add( pnlSingleCampusDiv );

            _ddlSingleAccountCampus = new ButtonDropDownList();
            _ddlSingleAccountCampus.ID = "_ddlSingleAccountCampus";
            pnlSingleCampusDiv.Controls.Add( _ddlSingleAccountCampus );

            var pnlAccountSingleDiv = new Panel() { CssClass = "" };
            _pnlAccountAmountEntrySingle.Controls.Add( pnlAccountSingleDiv );

            _ddlAccountSingle = new ButtonDropDownList();
            _ddlAccountSingle.ID = "_ddlAccountSingle";
            pnlAccountSingleDiv.Controls.Add( _ddlAccountSingle );

            /* Multi Account Mode*/

            _pnlAccountAmountEntryMulti = new Panel() { CssClass = "account-amount-multi-entry" };
            _pnlAccountAmountEntryMulti.ID = "_pnlAccountAmountEntryMulti";
            Controls.Add( _pnlAccountAmountEntryMulti );

            _rptPromptForAccountAmountsMulti = new Repeater();
            _rptPromptForAccountAmountsMulti.ID = "_rptPromptForAccountAmountsMulti";
            _rptPromptForAccountAmountsMulti.ItemDataBound += _rptPromptForAccountAmountsMulti_ItemDataBound;


            _rptPromptForAccountAmountsMulti.ItemTemplate = new PromptForAccountsMultiTemplate();
            _pnlAccountAmountEntryMulti.Controls.Add( _rptPromptForAccountAmountsMulti );

            _ddlMultiAccountCampus = new ButtonDropDownList();
            _ddlMultiAccountCampus.ID = "_ddlMultiAccountCampus";
            _pnlAccountAmountEntryMulti.Controls.Add( _ddlMultiAccountCampus );
        }

        /// <summary>
        /// 
        /// </summary>
        private class PromptForAccountsMultiTemplate : ITemplate
        {
            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn( Control container )
            {
                var itemTemplateControl = new Panel();
                itemTemplateControl.Controls.Add(
                    new HiddenField
                    {
                        ID = RepeaterControlIds.hfAccountAmountMultiAccountId
                    } );

                itemTemplateControl.Controls.Add(
                    new CurrencyBox
                    {
                        ID = RepeaterControlIds.nbAccountAmountMulti,
                        CssClass = "amount-input",
                        NumberType = ValidationDataType.Currency,
                        MinimumValue = "0"
                    } );

                container.Controls.Add( itemTemplateControl );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the _rptPromptForAccountAmountsMulti control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void _rptPromptForAccountAmountsMulti_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var financialAccount = e.Item.DataItem as FinancialAccount;
            if ( financialAccount == null )
            {
                return;
            }

            var hfAccountAmountMultiAccountId = e.Item.FindControl( RepeaterControlIds.hfAccountAmountMultiAccountId ) as HiddenField;
            var nbAccountAmountMulti = e.Item.FindControl( RepeaterControlIds.nbAccountAmountMulti ) as CurrencyBox;

            hfAccountAmountMultiAccountId.Value = financialAccount.Id.ToString();
            nbAccountAmountMulti.Label = financialAccount.PublicName;
        }
    }
}
