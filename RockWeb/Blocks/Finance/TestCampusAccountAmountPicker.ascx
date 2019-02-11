<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestCampusAccountAmountPicker.ascx.cs" Inherits="RockWeb.Blocks.Finance.TestCampusAccountAmountPicker" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <style>
            .account-amount-single-entry .amount-input {
                border: none;
                text-align: center;
                font-size: 60px;
            }

            .btn-group {
                margin-bottom: 15px;
            }
        </style>

        <h1>CampusAccountAmountPicker Inputs</h1>

        <Rock:AccountPicker ID="apSelectableAccounts" runat="server" Label="Selectable Accounts" AllowMultiSelect="true" OnSelectItem="apSelectableAccounts_SelectItem"  />

        <Rock:RockRadioButtonList ID="rblAmountEntryMode" runat="server" AutoPostBack="true" OnSelectedIndexChanged="rblAmountEntryMode_SelectedIndexChanged">
            <asp:ListItem Text="SingleAccount" Selected="True" />
            <asp:ListItem Text="MultipleAccounts" />
        </Rock:RockRadioButtonList>

        <Rock:AccountPicker ID="apSelectedAccount" runat="server" Label="Selected Account" AllowMultiSelect="false" OnSelectItem="apSelectedAccount_SelectItem" />

        <Rock:CurrencyBox ID="cbInputSingleAmount" runat="server" Label="Input Amount (Single)" AutoPostBack="true" OnTextChanged="cbInput_TextChanged" />

        <Rock:KeyValueList ID="kvlAmounts" runat="server" Label="Input Amounts (Multi)"  />
        <asp:LinkButton ID="btnSetMultiAccountAmounts" runat="server" Text="SetMultiAccountAmounts" CssClass="btn btn-xs btn-default" OnClick="btnSetMultiAccountAmounts_Click" />

        <hr />

        <h1>CampusAccountAmountPicker</h1>

        <Rock:CampusAccountAmountPicker Id="caapTest" runat="server" AutoPostBack="true" OnAccountChanged="caapTest_AccountChanged" OnCampusChanged="caapTest_CampusChanged"  />

        <hr />
        <h1>CampusAccountAmountPicker Output</h1>

        <Rock:RockLiteral ID="lSelectedAccount" runat="server" Label="Selected Account" />
        <Rock:RockLiteral ID="lSelectedCampus" runat="server" Label="Selected Campus" />
        <Rock:CurrencyBox ID="cbOutputAmount" runat="server" Label="Output Amount" AutoPostBack="true" ReadOnly="true" OnTextChanged="cbInput_TextChanged" />

    </ContentTemplate>
</asp:UpdatePanel>