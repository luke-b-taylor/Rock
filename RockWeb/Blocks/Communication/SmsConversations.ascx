<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsConversations.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsConversations" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments"></i> SMS Conversations</h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbNoNumbers" runat="server" NotificationBoxType="Warning" Text="No SMS numbers are available to view." Visible="false"></Rock:NotificationBox>

                <%-- The list of phone numbers that do not have "Enable Mobile Conversations" enabled --%>
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlSmsNumbers" runat="server" Label="SMS Number" AutoPostBack="true" OnSelectedIndexChanged="ddlSmsNumbers_SelectedIndexChanged"></Rock:RockDropDownList>
                        <asp:Label ID="lblSelectedSmsNumber" runat="server" visible="false" />
                    </div>
                    <div class="col-md-8">
                        <asp:LinkButton ID="btnCreateNewMessage" runat="server" CssClass="btn btn-primary pull-right rounded " OnClick="btnCreateNewMessage_Click"><i class="fa fa-comments"></i>&nbsp;Send SMS</asp:LinkButton>
                    </div>
                </div>
                <div class="row">

                    <div class="col-md-6 panel panel-block">
                        <asp:UpdatePanel ID="upRecipients" runat="server">
                            <ContentTemplate>
                                <Rock:Toggle ID="tglShowRead" runat="server" Label="Show Read" OnCheckedChanged="tglShowRead_CheckedChanged" OnText="Yes" OffText="No" Checked="true" />
                                <Rock:Grid ID="gRecipients" runat="server" OnRowSelected="gRecipients_RowSelected" OnRowDataBound="gRecipients_RowDataBound" ShowHeader="false" ShowActionRow="false">
                                    <Columns>
                                        <Rock:RockBoundField DataField="RecipientId" Visible="false"></Rock:RockBoundField>
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>
                                                <div class="row">
                                                    <div class="col-md-6">
                                                        <Rock:HiddenFieldWithClass ID="hfRecipientId" runat="server" CssClass="js-recipientId" Value='<%# Eval("RecipientId") %>' />
                                                        <Rock:HiddenFieldWithClass ID="hfMessageKey" runat="server" CssClass="js-messageKey" Value='<%# Eval("MessageKey") %>' />
                                                        <asp:Label ID="lblName" runat="server" Text='<%# Eval("FullName") ?? Eval("MessageKey") %>'></asp:Label>
                                                        <asp:LinkButton ID="lbLinkConversation" runat="server" Text="Link To Person" Visible="false" CssClass="btn btn-primary btn-sm" OnClick="lbLinkConversation_Click" CommandArgument='<%# Eval("MessageKey") %>'></asp:LinkButton>
                                                        <br />
                                                        <asp:Literal ID="litMessagePart" runat="server" Text='<%# Eval("LastMessagePart") %>'></asp:Literal>
                                                    </div>
                                                    <div class="col-md-6">
                                                        <asp:Literal ID="litDateTime" runat="server" Text='<%# Eval("CreatedDateTime") %>'></asp:Literal>
                                                        <br />
                                                        <asp:RadioButton ID="rbRead" runat="server" Checked='<%# !(bool)Eval("IsRead") %>' CssClass="js-read-checkbox" Enabled="false" />
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                    </Columns>
                                </Rock:Grid>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                
                    <div class="col-md-6 panel panel-block">
                        <asp:UpdatePanel ID="upConversation" runat="server"><ContentTemplate>
                            <Rock:HiddenFieldWithClass ID="hfSelectedRecipientId" runat="server" CssClass="js-selected-recipient-id" />
                            
                            <div class="row">
                                <div class="col-md-12">
                                    <asp:Repeater ID="rptConversation" runat="server" OnItemDataBound="rptConversation_ItemDataBound">
                                        <ItemTemplate>
                                            <div class="row">
                                                <Rock:HiddenFieldWithClass ID="hfCommunicationRecipientId" runat="server" Value='<%# Eval("FromPersonAliasId") %>' />
                                                <Rock:HiddenFieldWithClass ID="hfCommunicationMessageKey" runat="server" Value='<%# Eval("MessageKey") %>' />
                                                <div class="col-md-6 bg-primary pull-right" style="border-radius: 15px;  margin-bottom:15px;" id="divCommunication" runat="server">
                                                    <span class="small"> <%# Eval("CreatedDateTime") %></span><br />
                                                    <span><%# Eval("Response") %></span>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:Label ID="lbNoConversationsFound" runat="server" Visible='<%# rptConversation.Items.Count == 0 %>' Text="<tr><td>No conversations found.</td></tr>" CssClass="text-muted" />
                                        </FooterTemplate>
                                    </asp:Repeater>


                                </div>
                            </div>
                            <br />
                            <div class="row">
                                <div class="col-md-10"><Rock:RockTextBox ID="tbNewMessage" runat="server"></Rock:RockTextBox></div>
                                <div class="col-md-2"><Rock:BootstrapButton ID="btnSend" runat="server" CssClass="btn btn-primary js-send-text-button" Text="Send" OnClick="btnSend_Click"></Rock:BootstrapButton></div>
                            </div>

                        </ContentTemplate></asp:UpdatePanel>
                    </div>
                </div>

            </div>

        </div> <%-- End panel-block --%>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdNewMessage" runat="server" OnSaveClick="mdNewMessage_SaveClick" OnCancelScript="clearActiveDialog();" SaveButtonText="Send">
            <Content>
                <%-- person picker --%>
                <Rock:PersonPicker ID="ppRecipient" runat="server" Label="Recipient" />
                <%-- multi-line textbox --%>
                <Rock:RockControlWrapper ID="rcwSMSMessage" runat="server" Label="Message" Help="<span class='tip tip-lava'></span>">
                <asp:HiddenField ID="hfSMSCharLimit" runat="server" />
                <asp:Label ID="lblSMSMessageCount" runat="server" CssClass="badge margin-all-sm pull-right" />
                <Rock:RockTextBox ID="tbSMSTextMessage" runat="server" CssClass="js-sms-text-message" TextMode="MultiLine" Rows="3" Required="true" ValidationGroup="vgMobileTextEditor" RequiredErrorMessage="Message is required" ValidateRequestMode="Disabled" />
            </Rock:RockControlWrapper>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdLinkConversation" runat="server" OnSaveClick="mdLinkConversation_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <asp:HiddenField ID="hfMessageKey" runat="server" />
                <asp:HiddenField ID="hfActiveTab" runat="server" />

                <ul class="nav nav-pills margin-b-md">
                    <li id="liNewPerson" runat="server" class="active"><a href='#<%=divNewPerson.ClientID%>' data-toggle="pill">Add New Person</a></li>
                    <li id="liExistingPerson" runat="server"><a href='#<%=divExistingPerson.ClientID%>' data-toggle="pill">Add Existing Person</a></li>
                </ul>

                <Rock:NotificationBox ID="nbAddPerson" runat="server" Heading="Please correct the following:" NotificationBoxType="Danger" Visible="false" />
                <asp:ValidationSummary ID="valSummaryAddPerson" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="AddPerson"/>

                <div class="tab-content">

                    <div id="divNewPerson" runat="server" class="tab-pane active">
                        <div class="row">
                            <div class="col-sm-4">
                                <div class="well">
                                    <Rock:DefinedValuePicker ID="dvpNewPersonTitle" runat="server" Label="Title" ValidationGroup="AddPerson" CssClass="input-width-md" />
                                    <Rock:RockTextBox ID="tbNewPersonFirstName" runat="server" Label="First Name" ValidationGroup="AddPerson" Required="true" autocomplete="off" />
                                    <Rock:RockTextBox ID="tbNewPersonLastName" runat="server" Label="Last Name" ValidationGroup="AddPerson" Required="true" autocomplete="off" />
                                    <Rock:DefinedValuePicker ID="dvpNewPersonSuffix" runat="server" Label="Suffix" ValidationGroup="AddPerson" CssClass="input-width-md" />
                                </div>
                            </div>
                            <div class="col-sm-4">
                                <div class="well">
                                    <Rock:DefinedValuePicker ID="dvpNewPersonConnectionStatus" runat="server" Label="Connection Status" ValidationGroup="AddPerson" Required="true"/>
                                    <Rock:RockRadioButtonList ID="rblNewPersonRole" runat="server" Required="true" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Role" ValidationGroup="AddPerson"/>
                                    <Rock:RockRadioButtonList ID="rblNewPersonGender" runat="server" Required="true" Label="Gender" RepeatDirection="Horizontal" ValidationGroup="AddPerson"/>
                                </div>
                            </div>
                            <div class="col-sm-4">
                                <div class="well">
                                    <Rock:DatePicker ID="dpNewPersonBirthDate" runat="server" Label="Birthdate" ValidationGroup="AddPerson" AllowFutureDateSelection="False" ForceParse="false"/>
                                    <Rock:GradePicker ID="ddlGradePicker" runat="server" Label="Grade" ValidationGroup="AddPerson" UseAbbreviation="true" UseGradeOffsetAsValue="true" />
                                    <Rock:DefinedValuePicker ID="dvpNewPersonMaritalStatus" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Marital Status"  ValidationGroup="AddPerson"/>
                                </div>
                            </div>
                        </div>

                    </div>

                    <div id="divExistingPerson" runat="server" class="tab-pane">
                        <fieldset>
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" ValidationGroup="AddPerson" />
                        </fieldset>
                    </div>

                </div>

            </Content>
        </Rock:ModalDialog>

        <script>
            function clearActiveDialog() {
                $('#<%=hfActiveDialog.ClientID %>').val('');
            }
            Sys.Application.add_load(function () {
            var smsCharLimit = $('#<%=hfSMSCharLimit.ClientID%>').val();
                if ( smsCharLimit && smsCharLimit > 0)
                {
                    $('#<%=tbSMSTextMessage.ClientID%>').limit(
                        {
                            maxChars: smsCharLimit,
                            counter: '#<%=lblSMSMessageCount.ClientID%>',
                            normalClass: 'badge',
                            warningClass: 'badge-warning',
                            overLimitClass: 'badge-danger'
                        });
            }

            <%--
                $('.js-repeater-recipient-item').click(function () {
                    // Remove the selected styling from all items.
                    $('.js-repeater-recipient-item').removeClass('bg-primary').addClass('bg-light');

                    var item = $(this);

                    // Add the selected styling to this item
                    item.addClass('bg-primary');

                    // Uncheck the box since it has now been read
                    item.find('.js-read-checkbox').prop('checked', false);

                    // Get the recipient ID and set the selected recipient Id hidden field value
                    var recipientId = item.find('.js-recipientId').val();
                    item.closest('.js-selected-recipient-id').val(recipientId);

                    // Create the postback with args adn send.
                    var postbackArg = "recipient-id:" + recipientId;
                    window.location = "javascript:__doPostBack('<%=upRecipients.ClientID %>', '" + postbackArg + "')";
                }); --%>
                })
        </script>
    </ContentTemplate>
</asp:UpdatePanel>