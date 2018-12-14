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
                            <table class="table table-striped table-responsive table-no-border" >
                                <asp:Repeater ID="rptRecipients" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td>
                                                <asp:Label ID="lblName" runat="server" Text='<%# Eval("FullName") %>'></asp:Label>
                                                <asp:LinkButton ID="lbLinkConversation" runat="server" Text="Link To Person" Visible='<%# (string)Eval("FullName") == "Unknown"  %>' OnClick="lbLinkConversation_Click"></asp:LinkButton>
                                                <br />
                                                <asp:Literal ID="litMessagePart" runat="server" Text='<%# Eval("LastMessagePart") %>'></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="litDateTime" runat="server" Text='<%# Eval("CreatedDateTime") %>''></asp:Literal>
                                                <br />
                                                <asp:RadioButton ID="rbRead" runat="server" Checked='<%# Eval("IsRead") %>'' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <asp:Label ID="lbNoFilesFound" runat="server" Visible='<%# rptRecipients.Items.Count == 0 %>' Text="<tr><td>No conversations found.</td></tr>" CssClass="text-muted" />
                                    </FooterTemplate>
                                </asp:Repeater>
                            </table>
                        </ContentTemplate></asp:UpdatePanel>
                    </div>
                
                    <div class="col-md-6 panel panel-block">
                        <asp:UpdatePanel ID="upConversation" runat="server"><ContentTemplate>
                        <div>
                            <%-- Show the messages for the selected recipient here --%>
                        </div>
                        <div>
                            <Rock:RockTextBox ID="tbNewMessage" runat="server"></Rock:RockTextBox>
                            <Rock:BootstrapButton ID="btnSend" runat="server" Text="Send"></Rock:BootstrapButton>
                        </div>

                        </ContentTemplate></asp:UpdatePanel>
                    </div>

                </div>

            </div>

        </div>
    </ContentTemplate>
</asp:UpdatePanel>