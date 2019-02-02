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
using System.Web.UI;
using Newtonsoft.Json;
using RestSharp;
using Rock;
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

            Response.AppendHeader( "Access-Control-Allow-Origin", "*" );

            RockPage.AddScriptSrcToHead( this.Page, "gotnpgatewayTokenizer", "https://sandbox.gotnpgateway.com/tokenizer/tokenizer.js" );
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
                SetEnabledPaymentTypes();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

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

        // helper functional methods (like BindGrid(), etc.)

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
            var restClient = new RestClient( "https://sandbox.gotnpgateway.com" );
            RestRequest restRequest = new RestRequest( "api/transaction", Method.POST );
            restRequest.AddHeader( "Authorization", tbApiKey.Text );

            var transaction = new
            {
                type = "sale",
                amount = ( int ) ( cbAmount.Text.AsDecimal() * 100 ),
                payment_method = new
                {
                    token = hfResponseToken.Value
                }
            };

            restRequest.AddJsonBody( transaction );

            //HttpWebRequestElement httpWebRequestElement = new HttpWebRequestElement();
            //httpWebRequestElement.UseUnsafeHeaderParsing = false;

            var response = restClient.Execute( restRequest );

            var responseObject = JsonConvert.DeserializeObject( response.Content );
            ceSaleResponse.Text = responseObject.ToJson( Formatting.Indented );
        }
    }
}