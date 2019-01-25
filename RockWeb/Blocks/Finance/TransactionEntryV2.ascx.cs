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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
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
        Description = "The accounts to display. By default all active accounts with a Public Name will be displayed.",
        Order = 5 )]

    [BooleanField(
        "Additional Accounts",
        Key = AttributeKey.PromptForAdditionalAccounts,
        Description = "Should users be allowed to select additional accounts? If so, any active account with a Public Name value will be available.",
        TrueText = "Display option for selecting additional accounts",
        FalseText = "Don't display option",
        DefaultBooleanValue = true,
        Order = 6 )]

    [BooleanField(
        "Scheduled Transactions",
        Key = AttributeKey.AllowScheduledTransactions,
        Description = "If the selected gateway(s) allow scheduled transactions, should that option be provided to user.",
        TrueText = "Allow",
        FalseText = "Don't Allow",
        DefaultBooleanValue = true,
        Order = 7 )]

    [BooleanField(
        "Show Scheduled Gifts",
        Key = AttributeKey.ShowScheduledTransactions,
        Description = "If the person has any scheduled gifts, show a summary of their scheduled gifts.",
        DefaultBooleanValue = true,
        Order = 8 )]

    [TextField(
        "Gift Term",
        Key = AttributeKey.GiftTerm,
        DefaultValue = "Gift",
        Order = 9 )]

    #endregion Block Attributes
    public partial class TransactionEntryV2 : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string AccountsToDisplay = "AccountsToDisplay";

            public const string PromptForAdditionalAccounts = "PromptForAdditionalAccounts";

            public const string AllowImpersonation = "AllowImpersonation";

            public const string AllowScheduledTransactions = "AllowScheduledTransactions";

            public const string BatchNamePrefix = "BatchNamePrefix";

            public const string FinancialGateway = "FinancialGateway";

            public const string FinancialSourceType = "FinancialSourceType";

            public const string ShowScheduledTransactions = "ShowScheduledTransactions";

            public const string GiftTerm = "GiftTerm";
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

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
            if ( !LoadGateways() )
            {
                return;
            }

            pnlTransactionEntry.Visible = true;

            if ( this.GetAttributeValue( AttributeKey.ShowScheduledTransactions ).AsBoolean() )
            {
                lScheduledTransactionsTitle.Text = string.Format( "Scheduled {0}", ( this.GetAttributeValue( AttributeKey.GiftTerm ) ?? "Gift" ).Pluralize() );
                pnlScheduledTransactions.Visible = true;
                BindScheduledTransactions();
            }
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
        private bool LoadGateways()
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

            var testGatewayGuid = Rock.SystemGuid.EntityType.FINANCIAL_GATEWAY_TEST_GATEWAY.AsGuid();

            if ( _financialGateway.GetGatewayComponent().TypeGuid == testGatewayGuid )
            {
                ShowConfigurationMessage( NotificationBoxType.Warning, "Testing", "You are using the Test Financial Gateway. No actual amounts will be charged to your card or bank account." );
                return true;
            }

            pnlGatewayHelp.Visible = false;
            HideConfigurationMessage();

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
            lScheduledTransactionTitle.Text = financialScheduledTransaction.TransactionFrequencyValue.Value;

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

        protected void btnScheduledTransactionEdit_Click( object sender, EventArgs e )
        {

        }

        protected void btnScheduledTransactionDelete_Click( object sender, EventArgs e )
        {

        }

        #endregion Scheduled Gifts
    }
}