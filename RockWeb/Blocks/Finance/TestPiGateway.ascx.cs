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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json;
using Rock;
using Rock.Financial;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Temporary Block to Test the Pi Gateway.
    /// </summary>
    [DisplayName( "Test Pi Gateway" )]
    [Category( "Utility" )]
    [Description( "Temporary Block to Test the Pi Gateway." )]

    #region Block Attributes

    #endregion Block Attributes

    public partial class TestPiGateway : RockBlock
    {

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            var gateway = new Rock.TransNational.Pi.PiGateway();
            gateway.InitializeBlock( this, this.gatewayIFrameContainer );
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
                tbApiKey.Text = this.GetBlockUserPreference( "APIKey" );
                SetEnabledPaymentTypes();
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

        }

        #endregion

        #region Methods

        #endregion

        public void SetEnabledPaymentTypes()
        {
            List<string> enabledPaymentTypes = new List<string>();
            if ( cbCreditCard.Checked )
            {
                enabledPaymentTypes.Add( "card" );
            }

            if ( cbAch.Checked )
            {
                enabledPaymentTypes.Add( "ach" );
            }

            hfEnabledTypes.Value = enabledPaymentTypes.ToJson();
        }

        protected void cbCreditCard_CheckedChanged( object sender, EventArgs e )
        {
            SetEnabledPaymentTypes();
        }

        /// <summary>
        /// Handles the Click event of the btnProcessSale control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnProcessSale_Click( object sender, EventArgs e )
        {
            var gateway = new Rock.TransNational.Pi.PiGateway();
            var apiKey = tbApiKey.Text;
            var amount = cbAmount.Text.AsDecimal();
            var customerId = tbCustomerId.Text;
            var transactionResponse = gateway.PostTransaction( apiKey, amount, customerId );
            ceSaleResponse.Text = transactionResponse.ToJson( Formatting.Indented );
        }

        /// <summary>
        /// Handles the Click event of the btnCreateCustomer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreateCustomer_Click( object sender, EventArgs e )
        {
            var gateway = new Rock.TransNational.Pi.PiGateway();
            PaymentInfo paymentInfo = new PaymentInfo
            {
                FirstName = tbFirstName.Text,
                LastName = tbLastName.Text,
                Street1 = acAddress.Street1,
                Street2 = acAddress.Street2,
                City = acAddress.City,
                State = acAddress.State,
                PostalCode = acAddress.PostalCode,
                Country = acAddress.Country,
                Email = tbEmail.Text,
                Phone = pnbPhone.Text
            };

            Rock.TransNational.Pi.CreateCustomerResponse customerResponse = gateway.CreateCustomer( tbApiKey.Text, hfResponseToken.Value, paymentInfo );
            ceCreateCustomerResponse.Text = customerResponse.ToJson( Formatting.Indented );

            if ( customerResponse.Data == null )
            {
                tbCustomerId.Text = "";
            }
            else
            {
                tbCustomerId.Text = customerResponse.Data.Id;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnGetPlans control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGetPlans_Click( object sender, EventArgs e )
        {
            var gateway = new Rock.TransNational.Pi.PiGateway();
            var getPlansResponse = gateway.GetPlans( tbApiKey.Text );
            ceGetPlansResponse.Text = getPlansResponse.ToJson( Formatting.Indented );
        }

        /// <summary>
        /// Handles the Click event of the btnCreatePlan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreatePlan_Click( object sender, EventArgs e )
        {
            var gateway = new Rock.TransNational.Pi.PiGateway();
            Rock.TransNational.Pi.CreatePlanParameters planParameters = new Rock.TransNational.Pi.CreatePlanParameters
            {
                Name = tbPlanName.Text,
                Description = tbPlanDescription.Text,
                Amount = tbPlanAmount.Text.AsDecimal(),
                BillingCycleInterval = tbPlanBillingCycleInterval.Text.AsInteger(),
                BillingFrequency = ddlPlanBillingFrequency.SelectedValueAsEnum<Rock.TransNational.Pi.BillingFrequency>(),
                BillingDays = tbPlanBillingDays.Text,
                Duration = 0
            };

            var test = planParameters.ToJson( Formatting.Indented );

            var createPlanResponse = gateway.CreatePlan( tbApiKey.Text, planParameters );
            if ( createPlanResponse.Data != null )
            {
                tbPlanId.Text = createPlanResponse.Data.Id;
            }
            else
            {
                tbPlanId.Text = null;
            }

            ceCreatePlanResponse.Text = createPlanResponse.ToJson( Formatting.Indented );
        }

        /// <summary>
        /// Handles the Click event of the btnCreateSubscription control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCreateSubscription_Click( object sender, EventArgs e )
        {
            var gateway = new Rock.TransNational.Pi.PiGateway();
            Rock.TransNational.Pi.CreateSubscriptionParameters subscriptionParameters = new Rock.TransNational.Pi.CreateSubscriptionParameters
            {
                Customer = new Rock.TransNational.Pi.SubscriptionCustomer
                {
                    Id = tbCustomerId.Text
                },
                NextBillDate = tbSubscriptionNextBillDate.Text.AsDateTime().Value,
                PlanId = tbPlanId.Text,
                Description = tbSubscriptionDescription.Text,
                Amount = tbSubscriptionAmount.Text.AsDecimal(),
                BillingCycleInterval = tbSubscriptionBillingCycleInterval.Text.AsIntegerOrNull(),
                BillingFrequency = ddlSubscriptionBillingFrequency.SelectedValueAsEnumOrNull<Rock.TransNational.Pi.BillingFrequency>(),
                BillingDays = tbSubscriptionBillingDays.Text,
                Duration = 0
            };

            var test = subscriptionParameters.ToJson( Formatting.Indented );

            var createSubscriptionResponse = gateway.CreateSubscription( tbApiKey.Text, subscriptionParameters );
            if ( createSubscriptionResponse.Data != null )
            {
                tbCreateSubscriptionResponse_SubscriptionId.Text = createSubscriptionResponse.Data.Id;
            }
            else
            {
                tbCreateSubscriptionResponse_SubscriptionId.Text = null;
            }

            ceCreateSubscriptionResponse.Text = createSubscriptionResponse.ToJson( Formatting.Indented );
        }

        /// <summary>
        /// Handles the Click event of the btnGetCustomerTransactionStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGetCustomerTransactionStatus_Click( object sender, EventArgs e )
        {
            var gateway = new Rock.TransNational.Pi.PiGateway();
            var queryTransactionStatusRequest = new Rock.TransNational.Pi.QueryTransactionStatusRequest
            {
                CustomerIdSearch = new Rock.TransNational.Pi.QuerySearchString { ComparisonOperator = "=", SearchValue = tbCustomerId.Text }
            };


            var queryJson = queryTransactionStatusRequest.ToJson( Formatting.Indented );
            var response = gateway.QueryTransactionStatus( tbApiKey.Text, queryTransactionStatusRequest );

            ceQueryTransactionStatus.Text = response.ToJson( Formatting.Indented );
        }
    }
}