<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntryV2.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionEntryV2" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <%-- Friendly Guide if there is no Gateway configured --%>
        <asp:Panel ID="pnlGatewayGuide" runat="server" Visible="false">
        </asp:Panel>


        <asp:Panel ID="pnlTransactionEntry" runat="server">

            <div class="row">
                <%-- Scheduled Gifts Panel --%>
                <asp:Panel ID="pnlScheduledScheduledGifts" runat="server" CssClass="col-md-6">
                </asp:Panel>

                <%-- Collect Transaction Amount(s), etc --%>
                <asp:Panel ID="pnlPromptForAmounts" runat="server" CssClass="col-md-6">
                </asp:Panel>

                <asp:Panel ID="pnlPaymentInfo" runat="server" CssClass="col-md-6">
                    <%-- Placeholder for the Gateway's Payment Control --%>
                    <asp:PlaceHolder ID="phGatewayPaymentInfoControl" runat="server" />
                </asp:Panel>
            </div>
            <%-- Prompt for Accounts, Amounts --%>
        </asp:Panel>


    </ContentTemplate>
</asp:UpdatePanel>
