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
            }

            // handle custom postback events
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                // Grab the ID of the selected response to show the conversation history in the right pane.

            }
        }

        #endregion Control Overrides

        private void LoadPhoneNumbers()
        {
            // ddlSmsNumbers and select the first (newest communication?) number

            var SmsNumbers = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() )
                .DefinedValues
                .Where( v => v.GetAttributeValue( "EnableMobileConversations").AsBooleanOrNull() ?? true == false )
                .ToList();

            ddlSmsNumbers.DataSource = SmsNumbers;
            ddlSmsNumbers.DataValueField = "Id";
            ddlSmsNumbers.DataTextField = "Value";
            ddlSmsNumbers.DataBind();
        }

        private void LoadResponseListing()
        {
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
                    .GroupBy( r => new { r.RelatedCommunicationId, r.ToPersonAliasId } )
                    .Select( g => g.OrderByDescending( r => r.CreatedDateTime ).First() )
                    .ToList();

                if ( responses == null )
                {
                    return;
                }

                var responseListItems = responses
                    .Select( r => new ResponseListItem
                    {
                        FullName = r.PersonAlias.Person.FullName,
                        CreatedDateTime = r.CreatedDateTime,
                        LastMessagePart = r.Response.Truncate(25),
                        IsRead = r.IsRead
                    } )
                    .ToList();

                rptRecipients.DataSource = responseListItems;
                rptRecipients.DataBind();
            }
        }




        protected class ResponseListItem
        {
            public string FullName { get; set; }
            public DateTime? CreatedDateTime { get; set; }
            public string LastMessagePart { get; set; }
            public bool IsRead { get; set; }
        }


        #region Control Events
        protected void lbLinkConversation_Click( object sender, EventArgs e )
        {

        }

        protected void ddlSmsNumbers_SelectedIndexChanged( object sender, EventArgs e )
        {

        }

        protected void tglShowRead_CheckedChanged( object sender, EventArgs e )
        {

        }

        protected void btnCreateNewMessage_Click( object sender, EventArgs e )
        {

        }
        #endregion Control Events
    }
}