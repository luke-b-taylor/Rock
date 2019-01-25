<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntryV2.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionEntryV2" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <%-- Message for any configuration warnings --%>
        <Rock:NotificationBox ID="nbConfigurationNotification" runat="server" Visible="false" />


        <%-- Friendly Help if there is no Gateway configured --%>
        <asp:Panel ID="pnlGatewayHelp" runat="server" Visible="false">
            <h4>Welcome to Rock's On-line Giving Experience</h4>
            <p>There is currently no gateway configured. Below are a list of gateways installed on your server. You can also add additional gateways through the Rock Shop.</p>
            <asp:Repeater ID="rptInstalledGateways" runat="server" OnItemDataBound="rptInstalledGateways_ItemDataBound">
                <ItemTemplate>
                    <div class="panel panel-block">
                        <div class="panel-body">
                            <asp:HiddenField ID="hfGatewayId" runat="server" />
                            <h4>
                                <asp:Literal ID="lGatewayName" runat="server" /></h4>
                            <p>
                                <asp:Literal ID="lGatewayDescription" runat="server" />
                            </p>
                            <div class="actions">
                                <asp:LinkButton ID="btnGatewayConfigure" runat="server" CssClass="btn btn-xs btn-success" Text="Configure" OnClick="btnGatewayConfigure_Click" />
                                <asp:LinkButton ID="btnGatewayLearnMore" runat="server" CssClass="btn btn-xs btn-link" Text="Learn More" OnClick="btnGatewayLearnMore_Click" />
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:Panel ID="pnlTransactionEntry" runat="server">

            <div class="row">
                <%-- Scheduled Gifts Panel --%>
                <asp:Panel ID="pnlScheduledTransactions" runat="server" CssClass="col-sm-4 scheduled-transactions" Visible="false">
                    <h4><asp:Literal ID="lScheduledTransactionsTitle" runat="server" Text="Scheduled Transactions" /></h4>
                    <asp:Repeater ID="rptScheduledTransactions" runat="server" OnItemDataBound="rptScheduledTransactions_ItemDataBound">
                        <ItemTemplate>
                            <asp:HiddenField ID="hfScheduledTransactionId" runat="server" />
                            <div class="panel panel-block scheduled-transaction">
                                <div class="panel-heading">
                                    <h1 class="panel-title"><i class="fa fa-calendar"></i>
                                        <asp:Literal ID="lScheduledTransactionTitle" runat="server" /></h1>

                                    <div class="panel-actions pull-right">
                                        <span class="js-toggle-scheduled-details fa fa-plus"></span>
                                    </div>
                                </div>
                                <div class="panel-body">
                                    <div class="row">
                                        <div class="col-md-12">
                                            <asp:Repeater ID="rptScheduledTransactionAccounts" runat="server" OnItemDataBound="rptScheduledTransactionAccounts_ItemDataBound">
                                                <ItemTemplate>
                                                    <div class="row">
                                                        <div class="col-md-12">
                                                            <span class="scheduled-transaction-account control-label">
                                                                <asp:Literal ID="lScheduledTransactionAccountName" runat="server" /></span>
                                                            <br />
                                                            <span class="scheduled-transaction-amount">
                                                                <asp:Literal ID="lScheduledTransactionAmount" runat="server" /></span>
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </div>
                                    <div class="actions">
                                        <asp:LinkButton ID="btnScheduledTransactionEdit" runat="server" CssClass="btn btn-xs btn-link" Text="Edit" OnClick="btnScheduledTransactionEdit_Click" />
                                        <asp:LinkButton ID="btnScheduledTransactionDelete" runat="server" CssClass="btn btn-xs btn-link" Text="Delete" OnClick="btnScheduledTransactionDelete_Click" />
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

                <%-- Transaction Entry Panel --%>
                <div class="col-sm-8">
                    <%-- Collect Transaction Amount(s), etc --%>
                    <asp:Panel ID="pnlPromptForAmounts" runat="server">
                    </asp:Panel>

                    <asp:Panel ID="pnlPaymentInfo" runat="server">
                        <%-- Placeholder for the Gateway's Payment Control --%>
                        <asp:PlaceHolder ID="phGatewayPaymentInfoControl" runat="server" />
                    </asp:Panel>
                </div>
            </div>
            <%-- Prompt for Accounts, Amounts --%>
        </asp:Panel>


    </ContentTemplate>
</asp:UpdatePanel>
