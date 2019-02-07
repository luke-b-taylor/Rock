using Newtonsoft.Json;
using RestSharp;
using Rock.Financial;
using Rock.Web.UI;

// Use Newtonsoft RestRequest which is the same as RestSharp.RestRequest but uses the JSON.NET serializer
using RestRequest = RestSharp.Newtonsoft.Json.RestRequest;

namespace Rock.TransNational.Pi
{
    //[Description( "TransNational Pi Gateway" )]
    ///[Export( typeof( GatewayComponent ) )]
    //[ExportMetadata( "ComponentName", "TransNational Pi Gateway" )]
    public class PiGateway //: GatewayComponent
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

        #region Customers

        /// <summary>
        /// Creates the customer.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-new-customer
        /// NOTE: Pi Gateway supports multiple paymentment tokens per customer, but Rock will implement it as one Payment Method per Customer, and 0 or more Pi Customers per Rock Person.
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

        #endregion Customers

        #region Transactions

        /// <summary>
        /// Posts a transaction.
        /// https://sandbox.gotnpgateway.com/docs/api/#processing-a-transaction
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public CreateTransactionResponse PostTransaction( string apiKey, decimal? amount, string customerId )
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

            return response.Content.FromJsonOrNull<CreateTransactionResponse>();
        }

        #endregion Transactions

        #region Plans

        /// <summary>
        /// Creates the plan.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-plan
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="planParameters">The plan parameters.</param>
        /// <returns></returns>
        public CreatePlanResponse CreatePlan( string apiKey, CreatePlanParameters planParameters )
        {
            var restClient = new RestClient( "https://sandbox.gotnpgateway.com" );
            RestRequest restRequest = new RestRequest( "api/recurring/plan", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( planParameters );
            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<CreatePlanResponse>();
        }

        /// <summary>
        /// Deletes the plan.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        public string DeletePlan( string apiKey, string planId )
        {
            var restClient = new RestClient( "https://sandbox.gotnpgateway.com" );
            RestRequest restRequest = new RestRequest( $"api/recurring/plan/{planId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );
            var response = restClient.Execute( restRequest );

            return response.Content;
        }

        /// <summary>
        /// Gets the plans.
        /// https://sandbox.gotnpgateway.com/docs/api/#get-all-plans
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns></returns>
        public GetPlansResult GetPlans( string apiKey )
        {
            var restClient = new RestClient( "https://sandbox.gotnpgateway.com" );
            RestRequest restRequest = new RestRequest( "api/recurring/plans", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<GetPlansResult>();
        }

        #endregion Plans

        #region Subscriptions

        /// <summary>
        /// Creates the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-subscription
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        public CreateSubscriptionResponse CreateSubscription( string apiKey, CreateSubscriptionParameters subscriptionParameters )
        {
            var restClient = new RestClient( "https://sandbox.gotnpgateway.com" );
            RestRequest restRequest = new RestRequest( "api/recurring/subscription", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( subscriptionParameters );
            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<CreateSubscriptionResponse>();
        }

        #endregion Subscriptions

        #region utility

        // Enable/disable useUnsafeHeaderParsing. Hopefully this isn't needed
        // Derived from http://o2platform.wordpress.com/2010/10/20/dealing-with-the-server-committed-a-protocol-violation-sectionresponsestatusline/
        private void ToggleAllowUnsafeHeaderParsing( bool enable )
        {
            //Get the assembly that contains the internal class
            var webConfigSettings = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration( "~" );
            var systemNetConfiguration = webConfigSettings.GetSection( "system.net/settings" ) as System.Net.Configuration.SettingsSection;
            //systemNetConfiguration.HttpWebRequest.UseUnsafeHeaderParsing = enable;
        }

        #endregion utility
    }
}
