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
    [Description( "SMS Conversations between a SMS enabled phoen and a Rock SMS Phone number that does not have 'Enable Mobile Conversations' set to true." )]
    public partial class SmsConversations : RockBlock
    {
        #region Control Overrides

        protected void Page_Load( object sender, EventArgs e )
        {
            string postbackArgs = Request.Params["__EVENTARGUMENT"];

            if ( !IsPostBack )
            {
                LoadPhoneNumbers();
                LoadResponseListing();
            }

            // handle custom postback events
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                // Grab the ID of the selected response to show the conversation history in the right pane.
                string[] args = postbackArgs.Split( new char[] { '?' } );
                foreach ( string arg in args )
                {
                    string[] nameValue = arg.Split( new char[] { ':' } );
                    string eventParam = nameValue[0];

                    switch ( eventParam )
                    {
                        case "recipient-id":
                            int recipientId;
                            if ( !int.TryParse( nameValue[1], out recipientId ) )
                            {
                                break;
                            }

                            hfSelectedRecipientId.Value = nameValue[1];
                            LoadResponsesForRecipient( recipientId );
                            UpdateReadProperty( recipientId );
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #endregion Control Overrides

        #region private/protected Methods
        private void LoadPhoneNumbers()
        {
            // ddlSmsNumbers and select the first (newest communication?) number

            var SmsNumbers = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() )
                .DefinedValues
                .Where( v => v.GetAttributeValue( "EnableMobileConversations").AsBoolean( true ) == false )
                .ToList();

            ddlSmsNumbers.DataSource = SmsNumbers;
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

            using ( var rockContext = new RockContext() )
            {
                var responses = new CommunicationResponseService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( r => r.RelatedSmsFromDefinedValueId == smsPhoneDefinedValueId )
                    .GroupBy( r => new { r.FromPersonAliasId } )
                    .Select( g => g.OrderByDescending( r => r.CreatedDateTime ).FirstOrDefault() )
                    .ToList();

                if ( responses == null )
                {
                    return;
                }

                var responseListItems = responses
                    .Select( r => new ResponseListItem
                    {
                        RecipientId = r.FromPersonAliasId,
                        FullName = r.FromPersonAlias.Person.FullName,
                        CreatedDateTime = r.CreatedDateTime,
                        LastMessagePart = r.Response.Truncate(25),
                        IsRead = r.IsRead
                    } )
                    .ToList();
                rptRecipients.DataSource = responseListItems;
                rptRecipients.DataBind();
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

        protected class ResponseListItem
        {
            public int? RecipientId { get; set; }
            public string FullName { get; set; }
            public DateTime? CreatedDateTime { get; set; }
            public string LastMessagePart { get; set; }
            public bool IsRead { get; set; }
        }


        #endregion private/protected Methods


        #region Control Events
        protected void lbLinkConversation_Click( object sender, EventArgs e )
        {

        }

        protected void ddlSmsNumbers_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadResponseListing();
        }

        protected void tglShowRead_CheckedChanged( object sender, EventArgs e )
        {

        }

        protected void btnCreateNewMessage_Click( object sender, EventArgs e )
        {

        }

        protected void btnSend_Click( object sender, EventArgs e )
        {
            string smsMessage = tbNewMessage.Text.Trim();

            if (smsMessage.Length == 0 )
            {
                return;
            }

            // Create the SMS message and send it.

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            int personAliasId = hfSelectedRecipientId.ValueAsInt();
            var personAliasService = new PersonAliasService( new RockContext() );

            var phoneNumber = personAliasService
                .Queryable()
                .AsNoTracking()
                .Where( a => a.Id == personAliasId )
                .SelectMany( a => a.Person.PhoneNumbers )
                .Where( p => p.IsMessagingEnabled )
                .FirstOrDefault();

            string smsNumber = phoneNumber.Number;
            if ( !string.IsNullOrWhiteSpace( phoneNumber.CountryCode ) )
            {
                smsNumber = "+" + phoneNumber.CountryCode + phoneNumber.Number;
            }

            var recipient = new RecipientData( smsNumber, mergeFields );

            var person = personAliasService.GetPerson( personAliasId );
            if ( person != null )
            {
                recipient.MergeFields.Add( "Person", person );
            }

            var fromNumberDefinedValueCache = DefinedValueCache.Get( ddlSmsNumbers.SelectedValue.AsInteger() );

            var rockSMSMessage = new RockSMSMessage
            {
                CreateCommunicationRecord = true,
                Message = smsMessage,
                FromNumber = fromNumberDefinedValueCache
            };

            rockSMSMessage.AddRecipient( recipient );
            rockSMSMessage.Send();

            // create a communicationResponse to track the conversation
            //string errorMessage = string.Empty;
            //var sms = new Rock.Communication.Medium.Sms();
            //sms.ProcessResponse( smsNumber, fromNumberDefinedValueCache.Value, smsMessage, out errorMessage );

            //tbNewMessage.Text = string.Empty;

            LoadResponsesForRecipient( personAliasId );
        }

        #endregion Control Events




        protected void mdNewMessage_SaveClick( object sender, EventArgs e )
        {

        }

        protected void mdLinkConversation_SaveClick( object sender, EventArgs e )
        {

        }
    }
}