<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SmsConversations.ascx.cs" Inherits="RockWeb.Blocks.Communication.SmsConversations" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments"></i> SMS Conversations</h1>
            </div>

            <div class="panel-body">
                <%-- The list of phone numbers that do not have "Enable Mobile Conversations" enabled --%>
                <Rock:RockDropDownList ID="ddlSmsNumbers" runat="server" Label="SMS Number" AutoPostBack="true" OnSelectedIndexChanged="ddlSmsNumbers_SelectedIndexChanged"></Rock:RockDropDownList>
                
                <asp:LinkButton ID="btnCreateNewMessage" runat="server" CssClass="btn btn-primary pull-right rounded " OnClick="btnCreateNewMessage_Click"><i class="fa fa-comments"></i>&nbsp;Send SMS</asp:LinkButton>
        
                <div class="row">
                    <div class="col-md-6 panel panel-block">
                        <asp:UpdatePanel ID="upRecipients" runat="server"><ContentTemplate>
                            <Rock:Toggle ID="tglShowRead" runat="server" Label="Show Read" OnCheckedChanged="tglShowRead_CheckedChanged" />
                        <%-- Show the list of recipients here --%>
                                <asp:Repeater ID="rptRecipients" runat="server">
                                    <ItemTemplate>
                                        <div class="row border border-dark js-repeater-recipient-item">
                                            <div class="col-md-6">
                                                <Rock:HiddenFieldWithClass ID="hfRecipientId" runat="server" CssClass="js-recipientId" Value='<%# Eval("RecipientId") %>' />
                                                <asp:Label ID="lblName" runat="server" Text='<%# Eval("FullName") %>'></asp:Label>
                                                <asp:LinkButton ID="lbLinkConversation" runat="server" Text="Link To Person" Visible='<%# (string)Eval("FullName") == "Unknown"  %>' OnClick="lbLinkConversation_Click"></asp:LinkButton>
                                                <br />
                                                <asp:Literal ID="litMessagePart" runat="server" Text='<%# Eval("LastMessagePart") %>'></asp:Literal>
                                            </div>
                                            <div class="col-md-6">
                                                <asp:Literal ID="litDateTime" runat="server" Text='<%# Eval("CreatedDateTime") %>'></asp:Literal>
                                                <br />
                                                <asp:RadioButton ID="rbRead" runat="server" Checked='<%# !(bool)Eval("IsRead") %>' CssClass="js-read-checkbox" />
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:Label ID="lbNoRecipientsFound" runat="server" Visible='<%# rptRecipients.Items.Count == 0 %>' Text="<tr><td>No conversation recipients found.</td></tr>" CssClass="text-muted" />
                                    </FooterTemplate>
                                </asp:Repeater>
                        </ContentTemplate></asp:UpdatePanel>
                    </div>
                
                    <div class="col-md-6 panel panel-block">
                        <asp:UpdatePanel ID="upConversation" runat="server"><ContentTemplate>
                            <Rock:HiddenFieldWithClass ID="hfSelectedRecipientId" runat="server" CssClass="js-selected-recipient-id" />
                            
                            <div class="row">
                                <div class="col-md-12">
                                    <asp:Repeater ID="rptConversation" runat="server">
                                        <ItemTemplate>
                                            <div class="row border border-primary js-repeater-conversation-item">
                                                <div class="col-md-4">
                                                    <asp:Literal ID="litResponseDateTime" runat="server" Text='<%# Eval("CreatedDateTime") %>'></asp:Literal><br />
                                                    <div class="rounded js-repeater-response-item border border-primary"><asp:Literal ID="litResponse" runat="server" Text='<%# Eval("Response") %>'></asp:Literal></div>
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
                <%-- multi-line textbox --%>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdLinkConversation" runat="server" OnSaveClick="mdLinkConversation_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>
                <div class="row">
                    <div class="col-md-12">
                        <%-- new person existing person toggle --%>
                        <%-- person picker if existing is visible --%>
                    </div>
                </div>
                <div class="row">
                    <%-- only visible if new person is selected in the toggle --%>
                    <div class="col-md-4">
                        <%-- Title --%>
                        <%-- First Name --%>
                        <%-- Last Name --%>
                        <%-- Suffix --%>
                    </div>
                    <div class="col-md-4">
                        <%-- Connection Status --%>
                        <%-- Role --%>
                        <%-- Gender --%>
                    </div>
                    <div class="col-md-4">
                        <%-- Birthdate --%>
                        <%-- Grade --%>
                        <%-- Martial Status --%>
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <script>
            function clearActiveDialog() {
                $('#<%=hfActiveDialog.ClientID %>').val('');
            }

            Sys.Application.add_load(function () {
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
                });

            })
        </script>
    </ContentTemplate>
</asp:UpdatePanel>