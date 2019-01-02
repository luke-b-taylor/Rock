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
    public partial class SmsConversations : RockBlock
    {
        #region Control Overrides

        protected override void OnPreRender( EventArgs e)
        {
            base.OnPreRender( e );

            if ( mdLinkConversation.Visible )
            {
                string script = string.Format( @"

    $('#{0}').on('click', function () {{

        // if Save was clicked, set the fields that should be validated based on what tab they are on
        if ($('#{9}').val() == 'Existing') {{
            enableRequiredField( '{1}', true )
            enableRequiredField( '{2}_rfv', false );
            enableRequiredField( '{3}_rfv', false );
            enableRequiredField( '{4}', false );
            enableRequiredField( '{5}', false );
            enableRequiredField( '{6}_rfv', false );
            enableRequiredField( '{10}_rfv', false );
        }} else {{
            enableRequiredField('{1}', false)
            enableRequiredField('{2}_rfv', true);
            enableRequiredField('{3}_rfv', true);
            enableRequiredField('{4}', true);
            enableRequiredField('{5}', true);
            enableRequiredField('{6}_rfv', true);
            enableRequiredField('{10}_rfv', true);
        }}

        // update the scrollbar since our validation box could show
        setTimeout( function ()
        {{
            Rock.dialogs.updateModalScrollBar( '{7}' );
        }});

    }})

    $('a[data-toggle=""pill""]').on('shown.bs.tab', function (e) {{

        var tabHref = $( e.target ).attr( 'href' );
        if ( tabHref == '#{8}' )
        {{
            $( '#{9}' ).val( 'Existing' );
        }} else {{
            $( '#{9}' ).val( 'New' );
        }}

        // if the validation error summary is shown, hide it when they switch tabs
        $( '#{7}' ).hide();
    }});
",
                    mdLinkConversation.ServerSaveLink.ClientID,                         // {0}
                    ppPerson.RequiredFieldValidator.ClientID,                       // {1}
                    tbNewPersonFirstName.ClientID,                                  // {2}
                    tbNewPersonLastName.ClientID,                                   // {3}
                    rblNewPersonRole.RequiredFieldValidator.ClientID,               // {4}
                    rblNewPersonGender.RequiredFieldValidator.ClientID,             // {5}
                    dvpNewPersonConnectionStatus.ClientID,                          // {6}
                    valSummaryAddPerson.ClientID,                                   // {7}
                    divExistingPerson.ClientID,                                     // {8}
                    hfActiveTab.ClientID,                                           // {9}
                    dpNewPersonBirthDate.ClientID                                   // {10}
                );

                ScriptManager.RegisterStartupScript( mdLinkConversation, mdLinkConversation.GetType(), "modaldialog-validation", script, true );
            }
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
            // Create unique user setting keys for this block.
            //_settingKeyShowResults = _settingKeyShowResults.Replace( "{blockId}", this.BlockId.ToString() );

            btnCreateNewMessage.Visible = ( this.GetAttributeValue( "EnableSmsSend" ) ).AsBoolean();
            dvpNewPersonTitle.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ).Id;
            dvpNewPersonSuffix.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ).Id;
            dvpNewPersonMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ).Id;
            dvpNewPersonConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Id;

            var groupType = GroupTypeCache.GetFamilyGroupType();
            rblNewPersonRole.DataSource = groupType.Roles.OrderBy( r => r.Order ).ToList();
            rblNewPersonRole.DataBind();

            rblNewPersonGender.Items.Clear();
            rblNewPersonGender.Items.Add( new ListItem( Gender.Male.ConvertToString(), Gender.Male.ConvertToInt().ToString() ) );
            rblNewPersonGender.Items.Add( new ListItem( Gender.Female.ConvertToString(), Gender.Female.ConvertToInt().ToString() ) );
            rblNewPersonGender.Items.Add( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbAddPerson.Visible = false;

            if ( !IsPostBack )
            {
                if ( LoadPhoneNumbers() )
                {
                    nbNoNumbers.Visible = false;
                    LoadResponseListing();
                }
                else
                {
                    nbNoNumbers.Visible = true;
                }
            }
            else
            {
                ShowDialog();
            }

            if ( !string.IsNullOrWhiteSpace( hfActiveTab.Value ) )
            {
                SetActiveTab();
                mdLinkConversation.Show();
            }
        }

        #endregion Control Overrides

        #region private/protected Methods
        private bool LoadPhoneNumbers()
        {
            // First load up all of the available numbers
            var smsNumbers = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() )
                .DefinedValues
                .Where( v => v.GetAttributeValue( "EnableMobileConversations" ).AsBoolean( true ) == false )
                ;//.ToList();// probably do this last, keep here for testing

            var selectedNumberGuids = GetAttributeValue( "AllowedSMSNumbers" ).SplitDelimitedValues( true ).AsGuidList();
            if ( selectedNumberGuids.Any() )
            {
                smsNumbers = smsNumbers.Where( v => selectedNumberGuids.Contains( v.Guid ) ).ToList();
            }

            // filter personal numbers (any that have a response recipient) if the hide personal option is enabled
            if ( GetAttributeValue( "HidePersonalSmsNumbers" ).AsBoolean() )
            {
                smsNumbers = smsNumbers.Where( v => v.GetAttributeValue( "ResponseRecipient" ).IsNullOrWhiteSpace() ).ToList();
            }

            //show only numbers 'tied to the current' individual...unless they have 'Admin rights'.
            if ( GetAttributeValue( "ShowOnlyPersonalSmsNumber" ).AsBoolean() && !IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                smsNumbers = smsNumbers.Where( v => CurrentPerson.Aliases.Any( a => a.Guid == v.GetAttributeValue( "ResponseRecipient" ).AsGuid() ) ).ToList();
            }
            
            if ( smsNumbers.Any() )
            {
                ddlSmsNumbers.DataSource = smsNumbers.Select( v => new
                {
                    v.Id,
                    Description = string.IsNullOrWhiteSpace( v.Description ) ? v.Value : v.Description.Truncate( 100 ),
                });

                ddlSmsNumbers.DataValueField = "Id";
                ddlSmsNumbers.DataTextField = "Description";
                ddlSmsNumbers.DataBind();

                lblSelectedSmsNumber.Text = "SMS Number: " + ddlSmsNumbers.SelectedItem.Text.Truncate(25);
                lblSelectedSmsNumber.Visible = smsNumbers.Count() == 1;
                ddlSmsNumbers.Visible = smsNumbers.Count() > 1;

                string keyPrefix = string.Format( "sms-conversations-{0}-", this.BlockId );

                string smsNumberUserPref = this.GetUserPreference( keyPrefix + "smsNumber" ) ?? string.Empty;

                if ( smsNumberUserPref.IsNotNullOrWhiteSpace() )
                {
                    // Don't try to set the selected value unless you are sure it's in the list of items.
                    if ( ddlSmsNumbers.Items.FindByValue( smsNumberUserPref ) != null )
                    {
                        ddlSmsNumbers.SelectedValue = smsNumberUserPref;
                    }
                }

                tglShowRead.Checked = this.GetUserPreference( keyPrefix + "showRead" ).AsBooleanOrNull() ?? true;
            }
            else
            {
                return false;
            }

            return true;
        }

        private void LoadResponseListing()
        {
            // NOTE: The FromPersonAliasId is the person who sent a text from a mobile device to Rock.
            // This person is also referred to as the Recipient because they are responding to a
            // communication from Rock. Restated the response is from the recipient of a communication.

            int? smsPhoneDefinedValueId = ddlSmsNumbers.SelectedValue.AsIntegerOrNull();

            if ( smsPhoneDefinedValueId == null )
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

                // Since some of the responses will not have a PersonAlias associated with them
                // we need to group these by the MessageKey, which is the sending phone number
                // in the case of SMS messages.
                var filteredresponses = responses.GroupBy( r => new { r.MessageKey } )
                    .Select( g => g.OrderByDescending( r => r.CreatedDateTime ).FirstOrDefault() )
                    .ToList();

                if ( filteredresponses == null )
                {
                    return;
                }

                var responseListItems = filteredresponses
                    .Select( r => new ResponseListItem
                    {
                        RecipientId = r.FromPersonAliasId ?? -1,
                        MessageKey = r.MessageKey,
                        FullName = ( r.FromPersonAlias != null ? r.FromPersonAlias.Person.FullName : "Phone: " + r.MessageKey ),
                        CreatedDateTime = r.CreatedDateTime,
                        LastMessagePart = r.Response.LeftWithEllipsis(25),
                        IsRead = r.IsRead
                    } )
                    .ToList();

                rptConversation.Visible = false; // don't display conversations if we're rebinding the recipient list
                gRecipients.DataSource = responseListItems;
                gRecipients.DataBind();
            }
        }

        private void LoadResponsesForRecipient( int recipientId )
        {
            int? smsPhoneDefinedValueId = ddlSmsNumbers.SelectedValue.AsIntegerOrNull();

            if ( smsPhoneDefinedValueId == null )
            {
                return;
            }

            var communicationResponseService = new CommunicationResponseService( new RockContext() );
            var responses = communicationResponseService.GetConversation( recipientId, smsPhoneDefinedValueId.Value );

            rptConversation.Visible = true;
            rptConversation.DataSource = responses.Tables[0];
            rptConversation.DataBind();
        }

        private void LoadResponsesForRecipient( string messageKey )
        {
            int? smsPhoneDefinedValueId = ddlSmsNumbers.SelectedValue.AsIntegerOrNull();

            if ( smsPhoneDefinedValueId == null )
            {
                return;
            }

            var communicationResponseService = new CommunicationResponseService( new RockContext() );
            var responses = communicationResponseService.GetConversation( messageKey, smsPhoneDefinedValueId.Value );

            rptConversation.Visible = true;
            rptConversation.DataSource = responses.Tables[0];
            rptConversation.DataBind();
        }

        private void UpdateReadProperty( string messageKey )
        {
            int? smsPhoneDefinedValueId = ddlSmsNumbers.SelectedValue.AsIntegerOrNull();

            if ( smsPhoneDefinedValueId == null )
            {
                return;
            }

            new CommunicationResponseService( new RockContext() ).UpdateReadPropertyByMessageKey( messageKey, smsPhoneDefinedValueId.Value );
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
                    lblMdNewMessageSendingSMSNumber.Text = "SMS Number: " + ddlSmsNumbers.SelectedItem.Text.Truncate(25);
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

        private void SaveSettings()
        {
            string keyPrefix = string.Format( "sms-conversations-{0}-", this.BlockId );

            this.SetUserPreference( keyPrefix + "smsNumber", ddlSmsNumbers.SelectedValue.ToString(), false );
            this.SetUserPreference( keyPrefix + "showRead", tglShowRead.Checked.ToString(), false );
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
                if ( responseRecipientGuid.HasValue )
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

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( LoadPhoneNumbers() )
            {
                nbNoNumbers.Visible = false;
                LoadResponseListing();
            }
            else
            {
                nbNoNumbers.Visible = true;
            }
        }

        protected void lbLinkConversation_Click( object sender, EventArgs e )
        {
            var btn = ( LinkButton ) sender;
            hfMessageKey.Value = btn.CommandArgument;

            ShowDialog( "mdLinkConversation" );
        }

        protected void ddlSmsNumbers_SelectedIndexChanged( object sender, EventArgs e )
        {
            SaveSettings();
            LoadResponseListing();
        }

        protected void tglShowRead_CheckedChanged( object sender, EventArgs e )
        {
            SaveSettings();
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

        protected void gRecipients_RowSelected( object sender, RowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var recipientId = ( HiddenFieldWithClass ) e.Row.FindControl( "hfRecipientId" );
            var messageKey = ( HiddenFieldWithClass ) e.Row.FindControl( "hfMessageKey" );
            hfSelectedRecipientId.Value = recipientId.Value;
            
            if (recipientId.Value == "-1")
            {
                LoadResponsesForRecipient( messageKey.Value );
            }
            else
            {
                LoadResponsesForRecipient( recipientId.ValueAsInt() );
            }

            UpdateReadProperty( messageKey.Value );

            // Reset styling on all existing rows and set selected styling on this row
        }

        protected void gRecipients_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            HiddenFieldWithClass recipientId = ( HiddenFieldWithClass ) e.Row.FindControl( "hfRecipientId" );

            if ( recipientId.Value == "-1" )
            {
                LinkButton linkConversation = (LinkButton)e.Row.FindControl( "lbLinkConversation" );
                linkConversation.Visible = true;
            }
        }
        #endregion Control Events

        #region Link Conversation Modal
        private void SetActiveTab()
        {
            if ( hfActiveTab.Value == "Existing" )
            {
                liNewPerson.RemoveCssClass( "active" );
                divNewPerson.RemoveCssClass( "active" );
                liExistingPerson.AddCssClass( "active" );
                divExistingPerson.AddCssClass( "active" );
            }
            else
            {
                liNewPerson.AddCssClass( "active" );
                divNewPerson.AddCssClass( "active" );
                liExistingPerson.RemoveCssClass( "active" );
                divExistingPerson.RemoveCssClass( "active" );
            }
        }

        protected void mdLinkConversation_SaveClick( object sender, EventArgs e )
        {
            // Do some validation on entering a new person/family first
            if ( hfActiveTab.Value != "Existing" )
            {
                var validationMessages = new List<string>();
                bool isValid = true;
            
                DateTime? birthdate = dpNewPersonBirthDate.SelectedDate;
                if ( !dpNewPersonBirthDate.IsValid )
                {
                    validationMessages.Add( "Birthdate is not valid." );
                    isValid = false;
                }

                if ( !isValid )
                {
                    if ( validationMessages.Any() )
                    {
                        nbAddPerson.Text = "<ul><li>" + validationMessages.AsDelimited( "</li><li>" ) + "</li></lu>";
                        nbAddPerson.Visible = true;
                    }

                    return;
                }
            }
            using ( var rockContext = new RockContext() )
            {
                int mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

                if ( hfActiveTab.Value == "Existing" )
                {
                    if ( ppPerson.PersonId.HasValue )
                    {
                        // All we need to do here is add the mobile phone number and save
                        var personService = new PersonService( rockContext );
                        var person = personService.Get( ppPerson.PersonId.Value );
                        bool hasSmsNumber = person.PhoneNumbers.Where( p => p.IsMessagingEnabled ).Any();
                        var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobilePhoneTypeId );

                        if ( phoneNumber == null )
                        {
                            phoneNumber = new PhoneNumber
                            {
                                NumberTypeValueId = mobilePhoneTypeId,
                                IsMessagingEnabled = !hasSmsNumber,
                                Number = hfMessageKey.Value
                            };

                            person.PhoneNumbers.Add( phoneNumber );
                        }
                        else
                        {
                            phoneNumber.Number = hfMessageKey.Value;
                            if ( !hasSmsNumber )
                            {
                                //if they don't have a number then use this one, otherwise don't do anything
                                phoneNumber.IsMessagingEnabled = true;
                            }
                        }

                        rockContext.SaveChanges();
                        hfSelectedRecipientId.Value = person.PrimaryAliasId.ToString();
                    }
                }
                else
                {
                    // new Person and new family
                    var person = new Person();

                    person.TitleValueId = dvpNewPersonTitle.SelectedValueAsId();
                    person.FirstName = tbNewPersonFirstName.Text;
                    person.NickName = tbNewPersonFirstName.Text;
                    person.LastName = tbNewPersonLastName.Text;
                    person.SuffixValueId = dvpNewPersonSuffix.SelectedValueAsId();
                    person.Gender = rblNewPersonGender.SelectedValueAsEnum<Gender>();
                    person.MaritalStatusValueId = dvpNewPersonMaritalStatus.SelectedValueAsInt();


                    person.PhoneNumbers = new List<PhoneNumber>();
                    var phoneNumber = new PhoneNumber
                    {
                        NumberTypeValueId = mobilePhoneTypeId,
                        IsMessagingEnabled = true,
                        Number = hfMessageKey.Value
                    };

                    person.PhoneNumbers.Add( phoneNumber );

                    var birthMonth = person.BirthMonth;
                    var birthDay = person.BirthDay;
                    var birthYear = person.BirthYear;

                    var birthday = dpNewPersonBirthDate.SelectedDate;
                    if ( birthday.HasValue )
                    {
                        person.BirthMonth = birthday.Value.Month;
                        person.BirthDay = birthday.Value.Day;
                        if ( birthday.Value.Year != DateTime.MinValue.Year )
                        {
                            person.BirthYear = birthday.Value.Year;
                        }
                        else
                        {
                            person.BirthYear = null;
                        }
                    }
                    else
                    {
                        person.SetBirthDate( null );
                    }

                    person.GradeOffset = ddlGradePicker.SelectedValueAsInt();
                    person.ConnectionStatusValueId = dvpNewPersonConnectionStatus.SelectedValueAsId();

                    var groupMember = new GroupMember();
                    groupMember.GroupRoleId = rblNewPersonRole.SelectedValueAsInt() ?? 0;
                    groupMember.Person = person;

                    var groupMembers = new List<GroupMember>();
                    groupMembers.Add( groupMember );

                    Group group = GroupService.SaveNewFamily( rockContext, groupMembers, null, true );
                    hfSelectedRecipientId.Value = person.PrimaryAliasId.ToString();
                    
                }

                new CommunicationResponseService( rockContext ).UpdatePersonAliasByMessageKey( hfSelectedRecipientId.ValueAsInt(), hfMessageKey.Value, PersonAliasType.FromPersonAlias );
            }

            ppPerson.Required = false;
            tbNewPersonFirstName.Required = false;
            tbNewPersonLastName.Required = false;
            rblNewPersonRole.Required = false;
            rblNewPersonGender.Required = false;
            dvpNewPersonConnectionStatus.Required = false;

            hfActiveTab.Value = string.Empty;
            hfMessageKey.Value = string.Empty;
            
            mdLinkConversation.Hide();
            HideDialog();
            //upRecipients.Update();
            LoadResponseListing();
        }

        #endregion Link Conversation Modal

        protected void rptConversation_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var messageKey = ( HiddenFieldWithClass ) e.Item.FindControl( "hfCommunicationMessageKey" );
                if (messageKey.Value != string.Empty )
                {
                    var divCommunication = ( HtmlGenericControl ) e.Item.FindControl( "divCommunication" );
                    divCommunication.RemoveCssClass( "pull-right" );
                    divCommunication.AddCssClass( "pull-left" );
                    divCommunication.RemoveCssClass( "bg-primary" );
                    divCommunication.AddCssClass( "bg-info" );
                }
            }
        }
    }
}