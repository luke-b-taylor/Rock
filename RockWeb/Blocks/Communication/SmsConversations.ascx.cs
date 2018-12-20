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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls.Communication;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "SMS Conversations" )]
    [Category( "Core" )]
    [Description( "SMS Conversations between an SMS enabled phone and a Rock SMS Phone number that has 'Enable Mobile Conversations' set to false." )]
    [DefinedValueField( definedTypeGuid: Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
        name: "Allowed SMS Numbers",
        description: "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        required: false,
        allowMultiple: true,
        order: 1,
        key: "AllowedSMSNumbers" )]
    [BooleanField( "Show only personal SMS number",
        description: "Only SMS Numbers tied to the current individual will be shown. Those with ADMIN rights will see all SMS Numbers.",
        defaultValue: false,
        order: 2,
        key: "ShowOnlyPersonalSmsNumber" )]
    [BooleanField( "Hide personal SMS numbers",
        description: "Only SMS Numbers that are not associated with a person. The numbers without a 'ResponseRecipient' attribute value.",
        defaultValue: false,
        order: 3,
        key: "HidePersonalSmsNumbers" )]
    [BooleanField( "Enable SMS Send",
        description: "Allow SMS messages to be sent from the block.",
        defaultValue: true,
        order: 4,
        key: "EnableSmsSend" )]
    [IntegerField( "Character Limit",
        description: "Set this to show a character limit countdown for SMS communications. Set to 0 to disable",
        required: false,
        defaultValue: 160,
        order: 5,
        key: "CharacterLimit" )]
    public partial class SmsConversations : RockBlock
    {
        #region Control Overrides

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            hfSMSCharLimit.Value = ( this.GetAttributeValue( "CharacterLimit" ).AsIntegerOrNull() ?? 160 ).ToString();
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                LoadPhoneNumbers();
                LoadResponseListing();
            }
            else
            {
                ShowDialog();
            }
        }

        #endregion Control Overrides

        #region private/protected Methods
        private void LoadPhoneNumbers()
        {
            // First load up all of the available numbers
            var SmsNumbers = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() )
                .DefinedValues
                .Where( v => v.GetAttributeValue( "EnableMobileConversations").AsBoolean( true ) == false )
                .ToList();// probably do this last, keep here for testing

            var selectedNumberGuids = GetAttributeValue( "AllowedSMSNumbers" ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                SmsNumbers = SmsNumbers.Where( v => selectedNumberGuids.Contains( v.Guid ) ).ToList();
            }

            // show only current persons number
            if ( GetAttributeValue( "ShowOnlyPersonalSmsNumber" ).AsBoolean() )
            {
                var currentPersonSMSNumber = this.CurrentPerson.PhoneNumbers.FirstOrDefault( a => a.IsMessagingEnabled );
                SmsNumbers = SmsNumbers.Where( v => v.Value == currentPersonSMSNumber.Number ).ToList();
            }

            var filteredNumbers = new List<DefinedValueCache>();

            // hide personal numbers
            if ( GetAttributeValue( "HidePersonalSmsNumbers" ).AsBoolean() )
            {
                foreach( var number in SmsNumbers )
                {
                    if( number.GetAttributeValue( "ResponseRecipient" ).IsNullOrWhiteSpace() )
                    {
                        filteredNumbers.Add( number );
                    }
                }
                ddlSmsNumbers.DataSource = filteredNumbers;
            }
            else
            {
                ddlSmsNumbers.DataSource = SmsNumbers;
            }

            ddlSmsNumbers.DataValueField = "Id";
            ddlSmsNumbers.DataTextField = "Value";
            ddlSmsNumbers.DataBind();
        }

        private void LoadResponseListing()
        {
            // NOTE: The FromPersonAliasId is the person who sent a text from a mobile device to Rock.
            // This person is also referred to as the Recipient because they are responding to a
            // communication from Rock. Restated the the response is from the recipient of a communication.

            int? smsPhoneDefinedValueId = ddlSmsNumbers.SelectedValue.AsIntegerOrNull();

            if(smsPhoneDefinedValueId == null )
            {
                return;
            }

            bool showRead = tglShowRead.Checked;

            using ( var rockContext = new RockContext() )
            {
                var responses = new CommunicationResponseService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( r => r.RelatedSmsFromDefinedValueId == smsPhoneDefinedValueId );

                if ( !showRead )
                {
                    responses = responses.Where( r => r.IsRead == false );
                }

                var filteredresponses = responses.GroupBy( r => new { r.FromPersonAliasId } )
                    .Select( g => g.OrderByDescending( r => r.CreatedDateTime ).FirstOrDefault() )
                    .ToList();

                if ( filteredresponses == null )
                {
                    return;
                }

                var responseListItems = filteredresponses
                    .Select( r => new ResponseListItem
                    {
                        RecipientId = r.FromPersonAliasId,
                        MessageKey = r.MessageKey,
                        FullName = r.FromPersonAlias.Person.FullName,
                        CreatedDateTime = r.CreatedDateTime,
                        LastMessagePart = r.Response.Truncate(25),
                        IsRead = r.IsRead
                    } )
                    .ToList();

                gRecipients.DataSource = responseListItems;
                gRecipients.DataBind();
            }
        }

        private void LoadResponsesForRecipient( int recipientId )
        {
            int? smsPhoneDefinedValueId = ddlSmsNumbers.SelectedValue.AsIntegerOrNull();

            if(smsPhoneDefinedValueId == null )
            {
                return;
            }

            var communicationResponseService = new CommunicationResponseService( new RockContext() );
            var responses = communicationResponseService.GetConversation( recipientId, smsPhoneDefinedValueId.Value );

            rptConversation.DataSource = responses.Tables[0];
            rptConversation.DataBind();
        }

        private void UpdateReadProperty( int recipientId )
        {
            int? smsPhoneDefinedValueId = ddlSmsNumbers.SelectedValue.AsIntegerOrNull();

            if(smsPhoneDefinedValueId == null )
            {
                return;
            }

            new CommunicationResponseService( new RockContext() ).UpdateReadPropertyByFromPersonAliasId( recipientId, smsPhoneDefinedValueId.Value );
        }

        private Rock.Model.Communication CreateCommunication()
        {
            var communication = new Rock.Model.Communication();



            return communication;
        }

        protected class ResponseListItem
        {
            public int? RecipientId { get; set; }
            public string MessageKey { get; set; }
            public string FullName { get; set; }
            public DateTime? CreatedDateTime { get; set; }
            public string LastMessagePart { get; set; }
            public bool IsRead { get; set; }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "MDNEWMESSAGE":
                    mdNewMessage.Show();
                    break;
                case "MDLINKCONVERSATION":
                    mdLinkConversation.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "MDNEWMESSAGE":
                    mdNewMessage.Hide();
                    break;
                case "MDLINKCONVERSATION":
                    mdLinkConversation.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        private void SendMessage( int toPersonAliasId, string message )
        {
            using ( var rockContext = new RockContext() )
            {
                // default the sender to the logged in user. Only used if a ResponseRecipient is not defined for the SMS From number.
                int fromPersonAliasId = CurrentUser.Person.PrimaryAliasId.Value;
                string fromPersonName = CurrentUser.Person.FullName;

                // The sending phone is the selected one
                DefinedValueCache fromPhone = DefinedValueCache.Get( ddlSmsNumbers.SelectedValue.AsInteger() );

                var responseRecipientGuid = fromPhone.GetAttributeValue( "ResponseRecipient" ).AsGuidOrNull();
                if( responseRecipientGuid.HasValue )
                {
                    var fromPerson = new PersonAliasService( rockContext )
                        .Queryable().Where( p => p.Guid.Equals( responseRecipientGuid.Value ) )
                        .Select( p => p.Person )
                        .FirstOrDefault();

                    fromPersonAliasId = fromPerson.PrimaryAliasId.Value;
                    fromPersonName = fromPerson.FullName;
                }
                
                string responseCode = Rock.Communication.Medium.Sms.GenerateResponseCode( rockContext );

                // Create and enqueue the communication
                Rock.Communication.Medium.Sms.CreateCommunicationMobile( fromPersonAliasId, fromPersonName, toPersonAliasId, message, fromPhone, responseCode, rockContext );
            }
        }

        #endregion private/protected Methods


        #region Control Events
        protected void lbLinkConversation_Click( object sender, EventArgs e )
        {
            var btn = ( LinkButton ) sender;
            hfMessageKey.Value = btn.CommandArgument;

            ShowDialog( "mdLinkConversation" );
        }

        protected void ddlSmsNumbers_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadResponseListing();
        }

        protected void tglShowRead_CheckedChanged( object sender, EventArgs e )
        {
            LoadResponseListing();
        }

        protected void btnCreateNewMessage_Click( object sender, EventArgs e )
        {
            ShowDialog( "mdNewMessage" );
        }

        protected void btnSend_Click( object sender, EventArgs e )
        {
            string message = tbNewMessage.Text.Trim();

            if (message.Length == 0 )
            {
                return;
            }

            int toPersonAliasId = hfSelectedRecipientId.ValueAsInt();
            SendMessage( toPersonAliasId, message );
            tbNewMessage.Text = string.Empty;
            LoadResponsesForRecipient( toPersonAliasId );
        }

        protected void mdNewMessage_SaveClick( object sender, EventArgs e )
        {
            string message = tbSMSTextMessage.Text.Trim();
            if ( message.IsNullOrWhiteSpace() )
            {
                return;
            }

            int toPersonAliasId = ppRecipient.PersonAliasId.Value;
            SendMessage( toPersonAliasId, message );
            ppRecipient.SelectedValue = null;
            tbSMSTextMessage.Text = string.Empty;
            HideDialog();
        }

        protected void mdLinkConversation_SaveClick( object sender, EventArgs e )
        {



            hfMessageKey.Value = string.Empty;
            HideDialog();
        }

        protected void gRecipients_RowSelected( object sender, RowEventArgs e )
        {
            hfSelectedRecipientId.Value = e.RowKeyId.ToStringSafe();
            LoadResponsesForRecipient( e.RowKeyId );
            UpdateReadProperty( e.RowKeyId );
            // Reset styling on all existing rows and set selected styling on this row
        }

        #endregion Control Events

        protected void tNewOrExisting_CheckedChanged( object sender, EventArgs e )
        {

        }
    }
}