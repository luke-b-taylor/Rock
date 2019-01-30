// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Version 2 of the Transaction Entry Block
    /// </summary>
    [DisplayName( "Transaction Entry (V2)" )]
    [Category( "Finance" )]
    [Description( "Creates a new financial transaction or scheduled transaction." )]

    #region Block Attributes

    [FinancialGatewayField(
        "Financial Gateway",
        Key = AttributeKey.FinancialGateway,
        Description = "The payment gateway to use for Credit Card and ACH transactions.",
        Order = 0 )]

    [TextField(
        "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch.",
        DefaultValue = "Online Giving",
        Order = 2 )]

    [DefinedValueField(
        "Source",
        Key = AttributeKey.FinancialSourceType,
        Description = "The Financial Source Type to use when creating transactions.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        Order = 3 )]

    [BooleanField(
        "Impersonation",
        Key = AttributeKey.AllowImpersonation,
        Description = "Should the current user be able to view and edit other people's transactions? IMPORTANT: This should only be enabled on an internal page that is secured to trusted users.",
        TrueText = "Allow (only use on an internal page used by staff)",
        FalseText = "Don't Allow",
        DefaultBooleanValue = false,
        Order = 4 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKey.AccountsToDisplay,
        Description = "The accounts to display. By default all active accounts with a Public Name will be displayed. If the account has a child account for the selected campus, the child account for that campus will be used.",
        Order = 5 )]

    [BooleanField(
        "Scheduled Transactions",
        Key = AttributeKey.AllowScheduledTransactions,
        Description = "If the selected gateway(s) allow scheduled transactions, should that option be provided to user.",
        TrueText = "Allow",
        FalseText = "Don't Allow",
        DefaultBooleanValue = true,
        Order = 8 )]

    [BooleanField(
        "Show Scheduled Gifts",
        Key = AttributeKey.ShowScheduledTransactions,
        Description = "If the person has any scheduled gifts, show a summary of their scheduled gifts.",
        DefaultBooleanValue = true,
        Order = 9 )]

    [BooleanField(
        "Ask for Campus if Known",
        Key = AttributeKey.AskForCampusIfKnown,
        Description = "If the campus for the person is already known, should the campus still be prompted for?",
        DefaultBooleanValue = true,
        Order = 10 )]

    [BooleanField(
        "Enable Multi-Account",
        Key = AttributeKey.EnableMultiAccount,
        Description = "Should the person be able specify amounts for more than one account?",
        DefaultBooleanValue = true,
        Order = 11 )]

    [CodeEditorField(
        "Intro Message",
        Key = AttributeKey.IntroMessage,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text to place at the top of the amount entry",
        DefaultValue = "Your Generosity Changes Lives",
        Category = AttributeCategory.TextOptions,
        Order = 12 )]

    [TextField(
        "Gift Term",
        Key = AttributeKey.GiftTerm,
        DefaultValue = "Gift",
        Category = AttributeCategory.TextOptions,
        Order = 13 )]

    [TextField(
        "Give Button Text",
        Key = AttributeKey.GiveButtonText,
        DefaultValue = "Give Now",
        Category = AttributeCategory.TextOptions,
        Order = 14 )]

    [LinkedPage(
        "Scheduled Transaction Edit Page",
        Key = AttributeKey.ScheduledTransactionEditPage,
        Description = "The page to use for editing scheduled transactions.",
        Order = 15 )]

    [CodeEditorField(
        "Finish Lava Template",
        Key = AttributeKey.FinishLavaTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The text (HTML) to display on the success page.",
        DefaultValue = DefaultFinishLavaTemplate,
        Category = AttributeCategory.TextOptions,
        Order = 16 )]

    [TextField(
        "Save Account Title",
        Key = AttributeKey.SaveAccountTitle,
        Description = "The text to display as heading of section for saving payment information.",
        DefaultValue = "Make Giving Even Easier",
        Category = AttributeCategory.TextOptions,
        Order = 17 )]

    #endregion Block Attributes
    public partial class TransactionEntryV2 : RockBlock
    {
        #region constants

        public const string DefaultFinishLavaTemplate = @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

<h1>Thank You!</h1>

<p>Your support is helping Rock Solid Church Demo actively achieve our
mission. We are so grateful for your commitment.</p>

<dl>
    <dt>Confirmation Code</dt>
    <dd>{{ Transaction.TransactionCode }}</dd>

    <dt>Name</dt>
    <dd>{{ PaymentInfo.FullName }}</dd>
    <dd></dd>
    <dd>{{ PaymentInfo.Email }}</dd>
    <dd>{{ PaymentInfo.Street }} {{ PaymentInfo.City }}, {{ PaymentInfo.State }} {{ PaymentInfo.PostalCode }}</dd>
<dl>

<dl class='dl-horizontal'>
    {% for transactionDetail in transactionDetails %}
        <dt>{{ transactionDetail.Account.PublicName }}</dt>
        <dd>{{ transactionDetail.Amount }}</dd>
    {% endfor %}

    <dt>Payment Method</dt>
    <dd>{{ PaymentInfo.CurrencyTypeValue.Description}}</dd>

    {% if PaymentInfo.MaskedNumber != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentInfo.MaskedNumber }}</dd>
    {% endif %}

    <dt>When<dt>
    <dd>
    {% if PaymentSchedule %}
        {{ PaymentSchedule | ToString }}
    {% else %}
        Today
    {% endif %}
    </dd>
</dl>
";
        #endregion

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string AccountsToDisplay = "AccountsToDisplay";

            public const string AllowImpersonation = "AllowImpersonation";

            public const string AllowScheduledTransactions = "AllowScheduledTransactions";

            public const string BatchNamePrefix = "BatchNamePrefix";

            public const string FinancialGateway = "FinancialGateway";

            public const string FinancialSourceType = "FinancialSourceType";

            public const string ShowScheduledTransactions = "ShowScheduledTransactions";

            public const string ScheduledTransactionEditPage = "ScheduledTransactionEditPage";

            public const string GiftTerm = "GiftTerm";

            public const string GiveButtonText = "Give Button Text";

            public const string AskForCampusIfKnown = "AskForCampusIfKnown";

            public const string EnableMultiAccount = "EnableMultiAccount";

            public const string IntroMessage = "IntroMessage";

            public const string FinishLavaTemplate = "FinishLavaTemplate";

            public const string SaveAccountTitle = "SaveAccountTitle";
        }

        #endregion Attribute Keys

        #region Attribute Categories

        public static class AttributeCategory
        {
            public const string TextOptions = "Text Options";
        }

        #endregion Attribute Categories

        #region PageParameterKeys

        public static class PageParameterKey
        {
            public const string Person = "Person";
        }

        #endregion


        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            if ( !LoadGatewayOptions() )
            {
                return;
            }

            SetTargetPerson();

            lIntroMessage.Text = this.GetAttributeValue( AttributeKey.IntroMessage );

            LoadCampuses();
            BindAccounts();

            pnlTransactionEntry.Visible = true;
            bool enableMultiAccount = this.GetAttributeValue( AttributeKey.EnableMultiAccount ).AsBoolean();
            nbAccountAmountSingle.Visible = !enableMultiAccount;
            pnlPromptForAccountSingle.Visible = !enableMultiAccount;
            pnlPromptForAccountsMulti.Visible = enableMultiAccount;
            
            if ( this.GetAttributeValue( AttributeKey.ShowScheduledTransactions ).AsBoolean() )
            {
                lScheduledTransactionsTitle.Text = string.Format( "Scheduled {0}", ( this.GetAttributeValue( AttributeKey.GiftTerm ) ?? "Gift" ).Pluralize() );
                pnlScheduledTransactions.Visible = true;
                BindScheduledTransactions();
            }
            else
            {
                pnlScheduledTransactions.Visible = false;
            }

            UpdateGivingControlsForSelections();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        #endregion

        #region Methods

        #endregion

        #region Gateway Help Related

        /// <summary>
        /// Handles the Click event of the btnGatewayConfigure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGatewayConfigure_Click( object sender, EventArgs e )
        {
            // TODO
        }

        /// <summary>
        /// Handles the Click event of the btnGatewayLearnMore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGatewayLearnMore_Click( object sender, EventArgs e )
        {
            // TODO
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptInstalledGateways control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptInstalledGateways_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            FinancialGateway financialGateway = e.Item.DataItem as FinancialGateway;
            if ( financialGateway == null )
            {
                return;
            }

            HiddenField hfGatewayId = e.Item.FindControl( "hfGatewayId" ) as HiddenField;
            hfGatewayId.Value = financialGateway.Id.ToString();

            Literal lGatewayName = e.Item.FindControl( "lGatewayName" ) as Literal;
            Literal lGatewayDescription = e.Item.FindControl( "lGatewayDescription" ) as Literal;

            lGatewayName.Text = financialGateway.Name;
            lGatewayDescription.Text = financialGateway.Description;

            //LinkButton btnGatewayConfigure = e.Item.FindControl( "btnGatewayConfigure" ) as LinkButton;
            //LinkButton btnGatewayLearnMore = e.Item.FindControl( "btnGatewayLearnMore" ) as LinkButton;
        }

        /// <summary>
        /// Loads and Validates the gateways, showing a message if the gateways aren't configured correctly
        /// </summary>
        private bool LoadGatewayOptions()
        {
            var financialGatewayGuid = this.GetAttributeValue( AttributeKey.FinancialGateway ).AsGuidOrNull();
            var rockContext = new RockContext();
            var financialGatewayService = new FinancialGatewayService( rockContext );
            FinancialGateway _financialGateway = null;
            if ( financialGatewayGuid.HasValue )
            {
                _financialGateway = financialGatewayService.GetNoTracking( financialGatewayGuid.Value );
            }
            if ( _financialGateway == null )
            {
                ShowGatewayHelp();
                return false;
            }
            else
            {
                HideGatewayHelp();
            }

            var testGatewayGuid = Rock.SystemGuid.EntityType.FINANCIAL_GATEWAY_TEST_GATEWAY.AsGuid();

            if ( _financialGateway.GetGatewayComponent().TypeGuid == testGatewayGuid )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Testing", "You are using the Test Financial Gateway. No actual amounts will be charged to your card or bank account." );
            }
            else
            {

                HideConfigurationMessage();
            }

            var _financialGatewayComponent = _financialGateway.GetGatewayComponent();
            var supportedFrequencies = _financialGatewayComponent.SupportedPaymentSchedules;
            foreach ( var supportedFrequency in supportedFrequencies )
            {
                ddlFrequency.Items.Add( new ListItem( supportedFrequency.Value, supportedFrequency.Id.ToString() ) );
            }

            // If gateway didn't specifically support one-time, add it anyway for immediate gifts
            var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );
            if ( !supportedFrequencies.Where( f => f.Id == oneTimeFrequency.Id ).Any() )
            {
                ddlFrequency.Items.Insert( 0, new ListItem( oneTimeFrequency.Value, oneTimeFrequency.Id.ToString() ) );
            }

            ddlFrequency.SelectedValue = oneTimeFrequency.Id.ToString();
            dtpStartDate.SelectedDate = RockDateTime.Today;
            pnlScheduledTransaction.Visible = this.GetAttributeValue( AttributeKey.AllowScheduledTransactions ).AsBoolean();

            return true;
        }

        /// <summary>
        /// Shows the gateway help
        /// </summary>
        private void ShowGatewayHelp()
        {
            pnlGatewayHelp.Visible = true;
            pnlTransactionEntry.Visible = false;

            var rockContext = new RockContext();
            var installedGatewayList = new FinancialGatewayService( rockContext ).Queryable().OrderBy( a => a.Name ).AsNoTracking().ToList();

            rptInstalledGateways.DataSource = installedGatewayList;
            rptInstalledGateways.DataBind();
        }

        /// <summary>
        /// Hides the gateway help.
        /// </summary>
        private void HideGatewayHelp()
        {
            pnlGatewayHelp.Visible = false;
        }

        /// <summary>
        /// Shows the configuration message.
        /// </summary>
        /// <param name="notificationBoxType">Type of the notification box.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowConfigurationMessage( NotificationBoxType notificationBoxType, string title, string message )
        {
            nbConfigurationNotification.NotificationBoxType = notificationBoxType;
            nbConfigurationNotification.Title = title;
            nbConfigurationNotification.Text = message;

            nbConfigurationNotification.Visible = true;
        }

        /// <summary>
        /// Hides the configuration message.
        /// </summary>
        private void HideConfigurationMessage()
        {
            nbConfigurationNotification.Visible = false;
        }

        #endregion Gateway Guide Related

        #region Scheduled Gifts Related

        /// <summary>
        /// Binds the scheduled transactions.
        /// </summary>
        private void BindScheduledTransactions()
        {
            var rockContext = new RockContext();
            FinancialScheduledTransactionService financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );

            // get business giving id
            var givingIdList = CurrentPerson.GetBusinesses( rockContext ).Select( g => g.GivingId ).ToList();

            var currentPersonGivingId = CurrentPerson.GivingId;
            givingIdList.Add( currentPersonGivingId );
            var scheduledTransactionList = financialScheduledTransactionService.Queryable()
                .Where( a => givingIdList.Contains( a.AuthorizedPersonAlias.Person.GivingId ) && a.IsActive == true )
                .ToList();

            foreach ( var scheduledTransaction in scheduledTransactionList )
            {
                string errorMessage;
                financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessage );
            }

            rockContext.SaveChanges();

            pnlScheduledTransactions.Visible = scheduledTransactionList.Any();

            scheduledTransactionList = scheduledTransactionList.OrderByDescending( a => a.NextPaymentDate ).ToList();
            rptScheduledTransactions.DataSource = scheduledTransactionList;
            rptScheduledTransactions.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduledTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduledTransactions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            FinancialScheduledTransaction financialScheduledTransaction = e.Item.DataItem as FinancialScheduledTransaction;
            if ( financialScheduledTransaction == null )
            {
                return;
            }

            HiddenField hfScheduledTransactionId = e.Item.FindControl( "hfScheduledTransactionId" ) as HiddenField;
            Literal lScheduledTransactionTitle = e.Item.FindControl( "lScheduledTransactionTitle" ) as Literal;
            Literal lScheduledTransactionAmountTotal = e.Item.FindControl( "lScheduledTransactionAmountTotal" ) as Literal;
            hfScheduledTransactionId.Value = financialScheduledTransaction.Id.ToString();
            lScheduledTransactionTitle.Text = financialScheduledTransaction.TransactionFrequencyValue.Value;
            lScheduledTransactionAmountTotal.Text = financialScheduledTransaction.TotalAmount.FormatAsCurrency();

            Repeater rptScheduledTransactionAccounts = e.Item.FindControl( "rptScheduledTransactionAccounts" ) as Repeater;
            rptScheduledTransactionAccounts.DataSource = financialScheduledTransaction.ScheduledTransactionDetails.ToList();
            rptScheduledTransactionAccounts.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduledTransactionAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduledTransactionAccounts_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            FinancialScheduledTransactionDetail financialScheduledTransactionDetail = e.Item.DataItem as FinancialScheduledTransactionDetail;
            Literal lScheduledTransactionAccountName = e.Item.FindControl( "lScheduledTransactionAccountName" ) as Literal;
            lScheduledTransactionAccountName.Text = financialScheduledTransactionDetail.Account.ToString();

            Literal lScheduledTransactionAmount = e.Item.FindControl( "lScheduledTransactionAmount" ) as Literal;
            lScheduledTransactionAmount.Text = financialScheduledTransactionDetail.Amount.FormatAsCurrency();
        }

        /// <summary>
        /// Handles the Click event of the btnScheduledTransactionEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnScheduledTransactionEdit_Click( object sender, EventArgs e )
        {
            var scheduledTransactionId = ( ( sender as LinkButton ).NamingContainer.FindControl( "hfScheduledTransactionId" ) as HiddenField ).Value.AsInteger();
            NavigateToLinkedPage( AttributeKey.ScheduledTransactionEditPage, "ScheduledTransactionId", scheduledTransactionId );
        }

        /// <summary>
        /// Handles the Click event of the btnScheduledTransactionDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnScheduledTransactionDelete_Click( object sender, EventArgs e )
        {
            var namingContainer = ( sender as LinkButton ).NamingContainer;
            var scheduledTransactionId = ( namingContainer.FindControl( "hfScheduledTransactionId" ) as HiddenField ).Value.AsInteger();
            NotificationBox nbScheduledTransactionMessage = namingContainer.FindControl( "nbScheduledTransactionMessage" ) as NotificationBox;
            Panel pnlActions = namingContainer.FindControl( "pnlActions" ) as Panel;

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                FinancialScheduledTransactionService financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                var scheduledTransaction = financialScheduledTransactionService.Get( scheduledTransactionId );
                if ( scheduledTransaction == null )
                {
                    return;
                }

                scheduledTransaction.FinancialGateway.LoadAttributes( rockContext );

                string errorMessage = string.Empty;
                if ( financialScheduledTransactionService.Cancel( scheduledTransaction, out errorMessage ) )
                {
                    try
                    {
                        financialScheduledTransactionService.GetStatus( scheduledTransaction, out errorMessage );
                    }
                    catch
                    {
                        // ignore
                    }

                    rockContext.SaveChanges();

                    nbConfigurationNotification.Dismissable = true;
                    nbConfigurationNotification.NotificationBoxType = NotificationBoxType.Success;
                    nbScheduledTransactionMessage.Text = string.Format( "Your scheduled {0} has been deleted", GetAttributeValue( AttributeKey.GiftTerm ).ToLower() );
                    nbScheduledTransactionMessage.Visible = true;
                    pnlActions.Enabled = false;
                    pnlActions.Controls.OfType<LinkButton>().ToList().ForEach( a => a.Enabled = false );
                }
                else
                {
                    nbConfigurationNotification.Dismissable = true;
                    nbConfigurationNotification.NotificationBoxType = NotificationBoxType.Default;
                    nbScheduledTransactionMessage.Text = string.Format( "An error occurred while deleting your scheduled {0}", GetAttributeValue( AttributeKey.GiftTerm ).ToLower() );
                    nbConfigurationNotification.Details = errorMessage;
                    nbScheduledTransactionMessage.Visible = true;
                    pnlActions.Enabled = false;
                    pnlActions.Controls.OfType<LinkButton>().ToList().ForEach( a => a.Enabled = false );
                }
            }
        }

        #endregion Scheduled Gifts

        #region Transaction Entry Related

        /// <summary>
        /// Sets the target person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void SetTargetPerson()
        {
            // If impersonation is allowed, and a valid person key was used, set the target to that person

            Person targetPerson = null;

            if ( GetAttributeValue( AttributeKey.AllowImpersonation ).AsBoolean() )
            {
                string personKey = PageParameter( PageParameterKey.Person );
                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    var incrementKeyUsage = !this.IsPostBack;
                    var rockContext = new RockContext();
                    targetPerson = new PersonService( rockContext ).GetByImpersonationToken( personKey, incrementKeyUsage, this.PageCache.Id );

                    if ( targetPerson == null )
                    {
                        nbInvalidPersonWarning.Text = "Invalid or Expired Person Token specified";
                        nbInvalidPersonWarning.NotificationBoxType = NotificationBoxType.Danger;
                        nbInvalidPersonWarning.Visible = true;
                        return;
                    }
                }
            }

            if ( targetPerson == null )
            {
                targetPerson = CurrentPerson;
            }

            if ( targetPerson != null )
            {
                hfTargetPersonId.Value = targetPerson.Id.ToString();
            }
            else
            {
                hfTargetPersonId.Value = string.Empty;
            }
        }

        /// <summary>
        /// Binds the person saved accounts that are available for the <paramref name="selectedScheduleFrequencyId"/>
        /// </summary>
        /// <param name="selectedScheduleFrequencyId">The selected schedule frequency identifier.</param>
        private void BindPersonSavedAccounts( int selectedScheduleFrequencyId )
        {
            ddlPersonSavedAccount.Visible = false;
            var currentSavedAccountSelection = ddlPersonSavedAccount.SelectedValue;

            int? targetPersonId = hfTargetPersonId.Value.AsIntegerOrNull();
            if ( targetPersonId == null )
            {
                return;
            }

            var rockContext = new RockContext();
            var personSavedAccountsQuery = new FinancialPersonSavedAccountService( rockContext )
                .GetByPersonId( targetPersonId.Value )
                .Where( a => !a.IsSystem )
                .AsNoTracking();

            DefinedValueCache[] allowedCurrencyTypes = {
                DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid()),
                DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid())
                };

            int[] allowedCurrencyTypeIds = allowedCurrencyTypes.Select( a => a.Id ).ToArray();

            var financialGateway = this.GetFinancialGateway();
            if ( financialGateway == null )
            {
                return;
            }

            var financialGatewayComponent = financialGateway.GetGatewayComponent();
            if ( financialGatewayComponent == null )
            {
                return;
            }

            int oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0;
            bool oneTime = selectedScheduleFrequencyId == oneTimeFrequencyId;

            personSavedAccountsQuery = personSavedAccountsQuery.Where( a =>
                a.FinancialGatewayId == financialGateway.Id
                && ( a.FinancialPaymentDetail.CurrencyTypeValueId != null )
                && allowedCurrencyTypeIds.Contains( a.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );

            var personSavedAccountList = personSavedAccountsQuery.OrderBy( a => a.Name ).AsNoTracking().Select( a => new
            {
                a.Id,
                a.Name,
                a.FinancialPaymentDetail.AccountNumberMasked,
            } );

            ddlPersonSavedAccount.Visible = personSavedAccountList.Any();

            ddlPersonSavedAccount.Items.Clear();
            foreach ( var personSavedAccount in personSavedAccountList )
            {
                var displayName = string.Format( "{0} ({1})", personSavedAccount.Name, personSavedAccount.AccountNumberMasked );
                ddlPersonSavedAccount.Items.Add( new ListItem( displayName, personSavedAccount.Id.ToString() ) );
            }

            ddlPersonSavedAccount.Items.Add( new ListItem( "Use a different payment method", 0.ToString() ) );

            if ( currentSavedAccountSelection.IsNotNullOrWhiteSpace() )
            {
                ddlPersonSavedAccount.SetValue( currentSavedAccountSelection );
            }
            else
            {

                ddlPersonSavedAccount.SelectedIndex = 0;
            }
        }

        private FinancialGateway _financialGateway = null;

        /// <summary>
        /// Gets the financial gateway component that is configured for this block
        /// </summary>
        /// <returns></returns>
        private FinancialGateway GetFinancialGateway()
        {
            if ( _financialGateway == null )
            {
                RockContext rockContext = new RockContext();
                var financialGatewayGuid = this.GetAttributeValue( AttributeKey.FinancialGateway ).AsGuid();
                _financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayGuid );
            }

            return _financialGateway;
        }

        /// <summary>
        /// Loads the campuses.
        /// </summary>
        private void LoadCampuses()
        {
            ddlCampus.Items.Clear();
            foreach ( var campus in CampusCache.All().OrderBy( a => a.Order ) )
            {
                ddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }

            CampusCache defaultCampus = CampusCache.All().FirstOrDefault();

            if ( CurrentPerson != null )
            {
                var personCampus = CurrentPerson.GetCampus();
                if ( personCampus != null )
                {
                    defaultCampus = CampusCache.Get( personCampus.Id );
                }
            }

            if ( ddlCampus.Items.Count > 0 )
            {
                ddlCampus.SetValue( defaultCampus );
            }
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            var rockContext = new RockContext();
            var selectedAccountGuids = GetAttributeValues( AttributeKey.AccountsToDisplay ).AsGuidList();

            IQueryable<FinancialAccount> accountsQry;
            var financialAccountService = new FinancialAccountService( rockContext );

            if ( selectedAccountGuids.Any() )
            {
                accountsQry = financialAccountService.GetByGuids( selectedAccountGuids );
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

            ddlAccountSingle.Items.Clear();

            foreach ( var account in accountsList )
            {
                ddlAccountSingle.Items.Add( new ListItem( account.PublicName, account.Id.ToString() ) );
            }

            ddlAccountSingle.SetValue( accountsList.FirstOrDefault() );

            rptPromptForAccountsMulti.DataSource = accountsList;
            rptPromptForAccountsMulti.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPromptForAccountsMulti control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPromptForAccountsMulti_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var financialAccount = e.Item.DataItem as FinancialAccount;
            if ( financialAccount == null )
            {
                return;
            }

            var hfAccountAmountMultiAccountId = e.Item.FindControl( "hfAccountAmountMultiAccountId" ) as HiddenField;
            var nbAccountAmountMulti = e.Item.FindControl( "nbAccountAmountMulti" ) as CurrencyBox;

            hfAccountAmountMultiAccountId.Value = financialAccount.Id.ToString();
            nbAccountAmountMulti.Label = financialAccount.PublicName;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ddlFrequency control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void ddlFrequency_SelectionChanged( object sender, EventArgs e )
        {
            UpdateGivingControlsForSelections();
        }

        /// <summary>
        /// Updates the giving controls based on what options are selected in the UI
        /// </summary>
        private void UpdateGivingControlsForSelections()
        {
            BindPersonSavedAccounts( ddlFrequency.SelectedValue.AsInteger() );

            int selectedScheduleFrequencyId = ddlFrequency.SelectedValue.AsInteger();

            int oneTimeFrequencyId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid() ) ?? 0;
            bool oneTime = selectedScheduleFrequencyId == oneTimeFrequencyId;
            var giftTerm = this.GetAttributeValue( AttributeKey.GiftTerm );

            if ( oneTime )
            {
                dtpStartDate.Label = string.Format( "Process {0} On", giftTerm );
            }
            else
            {
                dtpStartDate.Label = "Start Giving On";
            }

            
            // if scheduling recurring, it can't start today since the gateway will be taking care of automated giving, it might have already processed today's transaction. So make sure it is no earlier than tomorrow.
            if ( !oneTime && ( !dtpStartDate.SelectedDate.HasValue || dtpStartDate.SelectedDate.Value.Date <= RockDateTime.Today ) )
            {
                dtpStartDate.SelectedDate = RockDateTime.Today.AddDays( 1 );
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ddlPersonSavedAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPersonSavedAccount_SelectionChanged( object sender, EventArgs e )
        {
            UpdateGivingControlsForSelections();
        }

        #endregion Transaction Entry Related
    }
}