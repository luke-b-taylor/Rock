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
                <Rock:Toggle ID="tglShowRead" runat="server" Label="Show Read" OnCheckedChanged="tglShowRead_CheckedChanged" />
                <asp:LinkButton ID="btnCreateNewMessage" runat="server" CssClass="btn btn-default" Text="New Message" OnClick="btnCreateNewMessage_Click" />
        
                <div class="row">
                    <div class="col-md-6 panel panel-block">
                        <asp:UpdatePanel ID="upRecipients" runat="server"><ContentTemplate>
                        <%-- Show the list of recipients here --%>
                                <asp:Repeater ID="rptRecipients" runat="server">
                                    <ItemTemplate>
                                        <div class="row js-repeater-item">
                                            <div class="col-md-6">
                                                <Rock:HiddenFieldWithClass ID="hfRecipientId" runat="server" CssClass="js-recipientId" />
                                                <asp:Label ID="lblName" runat="server" Text='<%# Eval("FullName") %>'></asp:Label>
                                                <asp:LinkButton ID="lbLinkConversation" runat="server" Text="Link To Person" Visible='<%# (string)Eval("FullName") == "Unknown"  %>' OnClick="lbLinkConversation_Click"></asp:LinkButton>
                                                <br />
                                                <asp:Literal ID="litMessagePart" runat="server" Text='<%# Eval("LastMessagePart") %>'></asp:Literal>
                                            </div>
                                            <div class="col-md-6">
                                                <asp:Literal ID="litDateTime" runat="server" Text='<%# Eval("CreatedDateTime") %>'></asp:Literal>
                                                <br />
                                                <asp:RadioButton ID="rbRead" runat="server" Checked='<%# !(bool)Eval("IsRead") %>' />
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:Label ID="lbNoFilesFound" runat="server" Visible='<%# rptRecipients.Items.Count == 0 %>' Text="<tr><td>No conversations found.</td></tr>" CssClass="text-muted" />
                                    </FooterTemplate>
                                </asp:Repeater>
                        </ContentTemplate></asp:UpdatePanel>
                    </div>
                
                    <div class="col-md-6 panel panel-block">
                        <asp:UpdatePanel ID="upConversation" runat="server"><ContentTemplate>
                            <asp:HiddenField ID="hfSelectedRecipientId" runat="server" />
                            <div class="row">
                                <div class="col">
                                    <%-- Show the messages for the selected recipient here --%>
                                    This is where the conversation history goes.
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-10"><Rock:RockTextBox ID="tbNewMessage" runat="server"></Rock:RockTextBox></div>
                                <div class="col-md-2"><Rock:BootstrapButton ID="btnSend" runat="server" CssClass="btn btn-primary" Text="Send"></Rock:BootstrapButton></div>
                            </div>

                        </ContentTemplate></asp:UpdatePanel>
                    </div>

                </div>

            </div>

        </div>

        <script>
            Sys.Application.add_load(function () {
                $('.js-repeater-item').click(function () {
                    var item = $(this);
                    var recipientId = item.find('.js-recipientId').val();
                    var postbackArg = "recipient-id:" + recipientId;

                    window.location = "javascript:__doPostBack('<%=upRecipients.ClientID %>', '" + postbackArg + "')";
                });
            })
        </script>
    </ContentTemplate>
</asp:UpdatePanel>