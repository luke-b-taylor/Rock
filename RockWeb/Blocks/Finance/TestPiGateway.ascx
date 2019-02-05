<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestPiGateway.ascx.cs" Inherits="RockWeb.Blocks.Finance.TestPiGateway" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <Rock:HiddenFieldWithClass ID="hfResponseToken" runat="server" CssClass="js-response-token" />
            <Rock:HiddenFieldWithClass ID="hfEnabledTypes" runat="server" CssClass="js-enabled-payment-types" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i>
                    Test Pi Gateway
                </h1>

            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-8">
                        <h1>Configure ACH/CC</h1>
                        <label>Gateway Config Attributes</label>
                        <Rock:RockTextBox ID="tbApiKey" runat="server" Label="Private API Key" Text="" Help="This would be a Gateway Config Attribute" />

                        <Rock:RockCheckBox ID="cbCreditCard" runat="server" Label="Credit Card" Checked="true" AutoPostBack="true" OnCheckedChanged="cbCreditCard_CheckedChanged" />
                        <Rock:RockCheckBox ID="cbAch" runat="server" Label="ACH" Checked="false" AutoPostBack="true" OnCheckedChanged="cbCreditCard_CheckedChanged" />

                        <h1>Get Amount</h1>
                        <Rock:CurrencyBox ID="cbAmount" runat="server" />

                        <hr />

                        <h1>Get Token</h1>

                        <input type="text" class="js-input-style-hook" style="display: none" />
                        <div id="gatewayIFrameContainer" class="margin-b-md" runat="server" style="border-color: red; border-style: solid; border-width: 1px"></div>

                        <span class="btn btn-primary btn-sm" onclick='submitTokenizer()'>Get Token</span>
                        <div class="">

                            <span class="control-label">Token Response</span>
                            <textarea class='js-response code' rows="5" cols="80" style="font-family: Courier New, Courier, monospace"></textarea>
                        </div>

                        <hr />
                        <Rock:NotificationBox ID="wbToken" runat="server" NotificationBoxType="Warning" Text="Note that Tokens can only be used once. (You'll get an 'Internal Server Error' if you use it more than once)." />
                        <h1>Create Customer</h1>

                        <Rock:RockLiteral ID="txtCurrentName" runat="server" Label="Name" Visible="false" />
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" Label="Address" />
                        <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Phone" />
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" />

                        <asp:LinkButton ID="btnCreateCustomer" runat="server" CssClass="btn btn-primary" Text="Create Customer" OnClick="btnCreateCustomer_Click" />
                        <Rock:CodeEditor ID="ceCreateCustomerResponse" runat="server" EditorMode="JavaScript" Label="Create Customer Response" EditorHeight="100" />
                        <Rock:RockTextBox ID="tbCustomerId" runat="server" Label="Customer Id" />

                        <h1>Process Sale</h1>
                        <asp:LinkButton ID="btnProcessSale" runat="server" CssClass="btn btn-primary" Text="Process Sale" OnClick="btnProcessSale_Click" />
                        <Rock:CodeEditor ID="ceSaleResponse" runat="server" EditorMode="JavaScript" Label="Sale Response" EditorHeight="400" />
                    </div>
                    <div class="col-md-4">
                        <h2>Developer API</h2>
                        <p>
                            <a href="https://sandbox.gotnpgateway.com/docs/api">Pi Developer Docs (api)</a>
                        </p>
                        <h2>Test Cards</h2>
                        <table class="grid-table table table-bordered table-striped table-hover">
                            <thead>
                                <tr>
                                    <th>Card Number</th>
                                    <th>Card Brand</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>4111111111111111</td>
                                    <td>visa</td>
                                </tr>
                                <tr>
                                    <td>4005519200000004</td>
                                    <td>visa</td>
                                </tr>
                                <tr>
                                    <td>4009348888881881</td>
                                    <td>visa</td>
                                </tr>
                                <tr>
                                    <td>4012000033330026</td>
                                    <td>visa</td>
                                </tr>
                                <tr>
                                    <td>4012000077777777</td>
                                    <td>visa</td>
                                </tr>
                                <tr>
                                    <td>4012888888881881</td>
                                    <td>visa</td>
                                </tr>
                                <tr>
                                    <td>4217651111111119</td>
                                    <td>visa</td>
                                </tr>
                                <tr>
                                    <td>4500600000000061</td>
                                    <td>visa</td>
                                </tr>
                                <tr>
                                    <td>5555555555554444</td>
                                    <td>mastercard</td>
                                </tr>
                                <tr>
                                    <td>2223000048400011</td>
                                    <td>mastercard</td>
                                </tr>
                                <tr>
                                    <td>378282246310005</td>
                                    <td>amex</td>
                                </tr>
                                <tr>
                                    <td>371449635398431</td>
                                    <td>amex</td>
                                </tr>
                                <tr>
                                    <td>6011111111111117</td>
                                    <td>discover</td>
                                </tr>
                                <tr>
                                    <td>36259600000004</td>
                                    <td>diners</td>
                                </tr>
                                <tr>
                                    <td>3530111333300000</td>
                                    <td>jcb</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>

            </div>

        </asp:Panel>

        <script type="text/javascript">

            var gatewayTokenizer;

            Sys.Application.add_load(function () {
                initializeTokenizer();
            });

            function initializeTokenizer() {

                var enabledPaymentTypes = JSON.parse($('.js-enabled-payment-types').val());;

                var $container = $('#<%=this.gatewayIFrameContainer.ClientID%>');

                var containerStyles = function (style) {
                    return $container.css(style);
                };
                var inputStyles = function (style) {
                    return $('.js-input-style-hook').css(style)
                };

                // create PI Gateway Tokenizer object
                gatewayTokenizer = new Tokenizer({
                    apikey: 'pub_1GVPn38YIiwClnuINKPc7uIB42A',
                    url: 'https://sandbox.gotnpgateway.com', // workaround for possible issue
                    container: $container[0],
                    submission: (resp) => {
                        debugger
                        $('.js-response-token').val(resp.token);
                        $('.js-response').val(JSON.stringify(resp, null, 2));
                    },
                    settings: {
                        payment: {
                            types: enabledPaymentTypes,
                            ach: {
                                sec_code: 'web' // Default web - web, ccd, ppd, tel
                            }
                        },
                        // Styles object will get converted into a css style sheet.
                        // Inspect elements to see structured html elements
                        // and style them the same way you would in css.
                        styles: {
                            'body': {
                                'color': containerStyles('color')
                            },
                            'input': {
                                'color': inputStyles('color'),
                                'border-radius': inputStyles('border-radius'),
                                'background-color': inputStyles('background-color'),
                                'border': inputStyles('border')
                            },
                            '.payment .cvv input': {
                                'border': inputStyles('border'),
                                'padding-left': inputStyles('padding-left')
                            }
                        }
                    }
                })

                // Initiate creation on container element
                gatewayTokenizer.create();
            }

            // Tells the gatewayTokenizer to submit the entered info so that we can get a token (or error, etc) in the response
            function submitTokenizer() {
                gatewayTokenizer.submit() // Use submission callback to deal with response
            }
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
