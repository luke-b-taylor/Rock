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
using System.Web.UI;
using Rock.Attribute;
using Rock;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Data;
using System.Linq;
using System.Data.Entity;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Test CampusAccountAmountPicker" )]
    [Category( "Finance" )]
    [Description( "" )]

    #region Block Attributes

    #endregion Block Attributes
    public partial class TestCampusAccountAmountPicker : RockBlock
    {

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
        }

        #endregion Attribute Keys


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
                apSelectableAccounts.SetValues( new[] { 1, 2 } );
                apSelectableAccounts_SelectItem( null, null );
                rblAmountEntryMode_SelectedIndexChanged( null, null );
                // added for your convenience

                // to show the created/modified by date time details in the PanelDrawer do something like this:
                // pdAuditDetails.SetEntity( <YOUROBJECT>, ResolveRockUrl( "~" ) );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblAmountEntryMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblAmountEntryMode_SelectedIndexChanged( object sender, EventArgs e )
        {
            caapTest.AmountEntryMode = rblAmountEntryMode.SelectedValue.ConvertToEnum<CampusAccountAmountPicker.AccountAmountEntryMode>();
            apSelectedAccount.Visible = caapTest.AmountEntryMode == CampusAccountAmountPicker.AccountAmountEntryMode.SingleAccount;
            UpdateSelectedCampusAndAccounts();
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

        /// <summary>
        /// Updates the selected campus and accounts.
        /// </summary>
        private void UpdateSelectedCampusAndAccounts()
        {
            var rockContext = new RockContext();
            var selectedAccountsQry = new FinancialAccountService( rockContext ).GetByIds( caapTest.SelectedAccountIds.ToList() );
            lSelectedAccount.Text = selectedAccountsQry.Select( a => a.Name ).ToList().AsDelimited( ", " );

            lSelectedCampus.Text = caapTest.CampusId.HasValue ? CampusCache.Get( caapTest.CampusId.Value ).Name : null;
        }

        /// <summary>
        /// Handles the AccountChanged event of the caapTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void caapTest_AccountChanged( object sender, EventArgs e )
        {
            UpdateSelectedCampusAndAccounts();
        }

        /// <summary>
        /// Handles the CampusChanged event of the caapTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void caapTest_CampusChanged( object sender, EventArgs e )
        {
            UpdateSelectedCampusAndAccounts();
        }

        #endregion

        #region Methods

        #endregion

        /// <summary>
        /// Handles the SelectItem event of the apSelectedAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void apSelectedAccount_SelectItem( object sender, EventArgs e )
        {
            caapTest.SelectedAccountId = apSelectedAccount.SelectedValueAsId();
            UpdateSelectedCampusAndAccounts();
        }

        /// <summary>
        /// Handles the SelectItem event of the apSelectableAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void apSelectableAccounts_SelectItem( object sender, EventArgs e )
        {
            caapTest.SelectableAccountIds = apSelectableAccounts.SelectedValuesAsInt().ToArray();
            UpdateSelectedCampusAndAccounts();
        }
    }
}