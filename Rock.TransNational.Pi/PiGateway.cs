using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Newtonsoft.Json;
using RestSharp;
using Rock.Financial;
using Rock.Web.UI;

namespace Rock.TransNational.Pi
{
    [Description( "TransNational Pi Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "TransNational Pi Gateway" )]
    public class PiGateway
    {
        /// <summary>
        /// Initializes the block.
        /// </summary>
        /// <param name="rockBlock">The rock block.</param>
        /// <param name="paymentContainer">The payment container.</param>
        public void InitializeBlock( RockBlock rockBlock, System.Web.UI.Control paymentContainer )
        {
            RockPage.AddScriptSrcToHead( rockBlock.Page, "gotnpgatewayTokenizer", "https://sandbox.gotnpgateway.com/tokenizer/tokenizer.js" );
            System.Web.UI.ScriptManager.RegisterStartupScript( rockBlock, rockBlock.GetType(), "piGatewayTokenizer", Scripts.gatewayTokenizer, true );
        }

        /// <summary>
        /// Creates the customer.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="tokenizerToken">The tokenizer token.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        public CreateCustomerResponse CreateCustomer( string apiKey, string tokenizerToken, PaymentInfo paymentInfo )
        {
            var restClient = new RestClient( "https://sandbox.gotnpgateway.com" );
            RestRequest restRequest = new RestRequest( "api/customer", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var createCustomer = new CreateCustomerRequest
            {
                description = "Test Description",
                payment_method = new PaymentMethodRequest( tokenizerToken ),
                billing_address = new BillingAddress
                {
                    first_name = paymentInfo.FirstName,
                    last_name = paymentInfo.LastName,
                    //company = "Some Business",
                    address_line_1 = paymentInfo.Street1,
                    address_line_2 = paymentInfo.Street2,
                    city = paymentInfo.City,
                    state = paymentInfo.State,
                    postal_code = paymentInfo.PostalCode,
                    country = paymentInfo.Country,
                    email = paymentInfo.Email,
                    phone = paymentInfo.Phone,
                    //fax =  555555555
                }
            };

            restRequest.AddJsonBody( createCustomer );

            ToggleAllowUnsafeHeaderParsing( true );

            var response = restClient.Execute( restRequest );

            var createCustomerResponse = JsonConvert.DeserializeObject<CreateCustomerResponse>( response.Content );
            return createCustomerResponse;
        }

        // Enable/disable useUnsafeHeaderParsing.
        // Derived from http://o2platform.wordpress.com/2010/10/20/dealing-with-the-server-committed-a-protocol-violation-sectionresponsestatusline/
        private void ToggleAllowUnsafeHeaderParsing( bool enable )
        {
            //Get the assembly that contains the internal class
            var webConfigSettings = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration( "~" );
            var systemNetConfiguration = webConfigSettings.GetSection( "system.net/settings" ) as System.Net.Configuration.SettingsSection;
            //systemNetConfiguration.HttpWebRequest.UseUnsafeHeaderParsing = enable;
        }

        /// <summary>
        /// Processes the sale.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public CreateTransactionResponse ProcessSale( string apiKey, decimal? amount, string customerId )
        {
            var restClient = new RestClient( "https://sandbox.gotnpgateway.com" );
            RestRequest restRequest = new RestRequest( "api/transaction", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var transaction = new Rock.TransNational.Pi.CreateTransaction
            {
                type = "sale",
                amount = ( int ) ( amount * 100 ),
                payment_method = new Rock.TransNational.Pi.PaymentMethodRequest
                {
                    Customer = new Rock.TransNational.Pi.PaymentMethodCustomer
                    {
                        id = customerId
                    }
                    //token = hfResponseToken.Value
                }
            };

            restRequest.AddJsonBody( transaction );

            var response = restClient.Execute( restRequest );

            return JsonConvert.DeserializeObject<CreateTransactionResponse>( response.Content, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore } );
        }
    }
}
