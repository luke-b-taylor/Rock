using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json;
using RestSharp;
using Rock.Attribute;
using Rock.Financial;
using Rock.Model;
using Rock.TransNational.Pi.Controls;
using Rock.Web.Cache;

// Use Newtonsoft RestRequest which is the same as RestSharp.RestRequest but uses the JSON.NET serializer
using RestRequest = RestSharp.Newtonsoft.Json.RestRequest;

namespace Rock.TransNational.Pi
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Financial.GatewayComponent" />
    [Description( "TransNational Pi Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "TransNational Pi Gateway" )]

    #region Component Attributes

    [TextField(
        "Private API Key",
        Key = AttributeKey.PrivateApiKey,
        Description = "The private API Key used for internal operations",
        Order = 1 )]

    [TextField(
        "Public API Key",
        Key = AttributeKey.PublicApiKey,
        Description = "The public API Key used for web client operations",
        Order = 2
        )]

    [TextField(
        "Gateway URL",
        Key = AttributeKey.GatewayUrl,
        Description = "The base URL of the gateway. For example: https://app.gotnpgateway.com for production or https://sandbox.gotnpgateway.com for testing",
        Order = 3
        )]

    #endregion Component Attributes
    public class PiGateway : GatewayComponent, IHostedGatewayComponent
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Component Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string PrivateApiKey = "PrivateApiKey";
            public const string PublicApiKey = "PublicApiKey";
            public const string GatewayUrl = "GatewayUrl";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        /// <value>
        /// The gateway URL.
        /// </value>
        public string GatewayUrl
        {
            get
            {
                return this.GetAttributeValue( AttributeKey.GatewayUrl );
            }
        }

        /// <summary>
        /// Gets the public API key.
        /// </summary>
        /// <value>
        /// The public API key.
        /// </value>
        public string PublicApiKey
        {
            get
            {
                return this.GetAttributeValue( AttributeKey.PublicApiKey );
            }
        }

        /// <summary>
        /// Gets the private API key.
        /// </summary>
        /// <value>
        /// The private API key.
        /// </value>
        private string PrivateApiKey
        {
            get
            {
                return this.GetAttributeValue( AttributeKey.PrivateApiKey );
            }
        }

        #region IHostedGatewayComponent

        /// <summary>
        /// Gets the hosted payment information control which will be used to collect CreditCard, ACH fields
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="controlId">The control identifier.</param>
        /// <returns></returns>
        public Control GetHostedPaymentInfoControl( FinancialGateway financialGateway, string controlId )
        {
            PiHostedPaymentControl piHostedPaymentControl = new PiHostedPaymentControl { ID = controlId };
            piHostedPaymentControl.PiGateway = this;

            return piHostedPaymentControl;
        }

        /// <summary>
        /// Gets the paymentInfoToken that the hostedPaymentInfoControl returned (see also <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.String)" />)
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        public string GetHostedPaymentInfoToken( FinancialGateway financialGateway, Control hostedPaymentInfoControl )
        {
            return ( hostedPaymentInfoControl as PiHostedPaymentControl ).PaymentInfoToken;
        }

        /// <summary>
        /// Gets the JavaScript needed to tell the hostedPaymentInfoControl to get send the paymentInfo and get a token
        /// Put this on your 'Next' or 'Submit' button so that the hostedPaymentInfoControl will fetch the token/response
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        public string GetHostPaymentInfoSubmitScript( FinancialGateway financialGateway, Control hostedPaymentInfoControl )
        {
            return $"submitTokenizer('{hostedPaymentInfoControl.ClientID}');";
        }

        #endregion IHostedGatewayComponent

        #region Temp Methods

        #region Customers

        /// <summary>
        /// Creates the customer.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-new-customer
        /// NOTE: Pi Gateway supports multiple payment tokens per customer, but Rock will implement it as one Payment Method per Customer, and 0 or more Pi Customers per Rock Person.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="tokenizerToken">The tokenizer token.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <returns></returns>
        private CreateCustomerResponse CreateCustomer( string apiKey, string tokenizerToken, PaymentInfo paymentInfo )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( "api/customer", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var createCustomer = new CreateCustomerRequest
            {
                Description = paymentInfo.FullName,
                PaymentMethod = new PaymentMethodRequest( tokenizerToken ),
                BillingAddress = new BillingAddress
                {
                    FirstName = paymentInfo.FirstName,
                    LastName = paymentInfo.LastName,
                    AddressLine1 = paymentInfo.Street1,
                    AddressLine2 = paymentInfo.Street2,
                    City = paymentInfo.City,
                    State = paymentInfo.State,
                    PostalCode = paymentInfo.PostalCode,
                    Country = paymentInfo.Country,
                    Email = paymentInfo.Email,
                    Phone = paymentInfo.Phone,
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
        /// <param name="type">The type (sale, authorize, credit)</param>
        /// <param name="amount">The amount.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private CreateTransactionResponse PostTransaction( string apiKey, TransactionType type, decimal amount, string customerId )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( "api/transaction", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            var transaction = new Rock.TransNational.Pi.CreateTransaction
            {
                Type = type,
                Amount = amount,
                PaymentMethod = new Rock.TransNational.Pi.PaymentMethodRequest( new Rock.TransNational.Pi.PaymentMethodCustomer( customerId ) )
            };

            restRequest.AddJsonBody( transaction );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<CreateTransactionResponse>();
        }

        /// <summary>
        /// Gets the transaction status.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private TransactionStatusResponse GetTransactionStatus( string apiKey, string transactionId )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<TransactionStatusResponse>();
        }

        /// <summary>
        /// Posts the void.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private TransactionVoidRefundResponse PostVoid( string apiKey, string transactionId )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}/void", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<TransactionVoidRefundResponse>();
        }

        /// <summary>
        /// Posts the refund.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns></returns>
        private TransactionVoidRefundResponse PostRefund( string apiKey, string transactionId, decimal amount )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/transaction/{transactionId}/refund", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var refundRequest = new TransactionRefundRequest { Amount = amount };
            restRequest.AddJsonBody( refundRequest );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<TransactionVoidRefundResponse>();
        }

        #endregion Transactions

        #region Plans

        /// <summary>
        /// Updates the billing plan BillingFrequency, BillingCycleInterval, BillingDays and Duration
        /// </summary>
        /// <param name="billingPlanParameters">The billing plan parameters.</param>
        /// <param name="scheduleTransactionFrequencyValueGuid">The schedule transaction frequency value unique identifier.</param>
        private static void SetBillingPlanParameters( BillingPlanParameters billingPlanParameters, Guid scheduleTransactionFrequencyValueGuid )
        {
            BillingFrequency? billingFrequency = null;
            int billingCycleInterval = 1;
            string billingDays = null;
            if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY.AsGuid() )
            {
                billingFrequency = BillingFrequency.monthly;
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY.AsGuid() )
            {
                billingFrequency = BillingFrequency.twice_monthly;
                billingDays = "1,15";
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY.AsGuid() )
            {
                //billingFrequency = BillingFrequency.daily;
            }
            else if ( scheduleTransactionFrequencyValueGuid == Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY.AsGuid() )
            {
                //billingFrequency = BillingFrequency.daily;
            }

            billingPlanParameters.BillingFrequency = billingFrequency;
            billingPlanParameters.BillingCycleInterval = billingCycleInterval;
            billingPlanParameters.BillingDays = billingDays;
            billingPlanParameters.Duration = 0;
        }

        /// <summary>
        /// Creates the plan.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-plan
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="planParameters">The plan parameters.</param>
        /// <returns></returns>
        private CreatePlanResponse CreatePlan( string apiKey, CreatePlanParameters planParameters )
        {
            var restClient = new RestClient( this.GatewayUrl );
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
        private string DeletePlan( string apiKey, string planId )
        {
            var restClient = new RestClient( this.GatewayUrl );
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
        private GetPlansResult GetPlans( string apiKey )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( "api/recurring/plans", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<GetPlansResult>();
        }

        #endregion Plans

        #region Transaction Query

        /// <summary>
        /// Returns a list of Transactions that meet the queryTransactionStatusRequest parameters
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="queryTransactionStatusRequest">The query transaction status request.</param>
        /// <returns></returns>
        private TransactionSearchResult SearchTransactions( string apiKey, QueryTransactionStatusRequest queryTransactionStatusRequest )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( "api/transaction/search", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( queryTransactionStatusRequest );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<TransactionSearchResult>();
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// Creates the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#create-a-subscription
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        private SubscriptionResponse CreateSubscription( string apiKey, SubscriptionRequestParameters subscriptionParameters )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( "api/recurring/subscription", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( subscriptionParameters );
            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<SubscriptionResponse>();
        }

        /// <summary>
        /// Updates the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#update-a-subscription
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        private SubscriptionResponse UpdateSubscription( string apiKey, string subscriptionId, SubscriptionRequestParameters subscriptionParameters )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.POST );
            restRequest.AddHeader( "Authorization", apiKey );

            restRequest.AddJsonBody( subscriptionParameters );
            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<SubscriptionResponse>();
        }

        /// <summary>
        /// Deletes the subscription.
        /// https://sandbox.gotnpgateway.com/docs/api/#delete-a-subscription
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="subscriptionParameters">The subscription parameters.</param>
        /// <returns></returns>
        private SubscriptionResponse DeleteSubscription( string apiKey, string subscriptionId )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.DELETE );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<SubscriptionResponse>();
        }

        /// <summary>
        /// Gets the subscription.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        private SubscriptionResponse GetSubscription( string apiKey, string subscriptionId )
        {
            var restClient = new RestClient( this.GatewayUrl );
            RestRequest restRequest = new RestRequest( $"api/recurring/subscription/{subscriptionId}", Method.GET );
            restRequest.AddHeader( "Authorization", apiKey );

            var response = restClient.Execute( restRequest );

            return response.Content.FromJsonOrNull<SubscriptionResponse>();
        }

        #endregion Subscriptions

        #region utility

        // Enable/disable useUnsafeHeaderParsing. Hopefully this isn't needed
        // Derived from http://o2platform.wordpress.com/2010/10/20/dealing-with-the-server-committed-a-protocol-violation-sectionresponsestatusline/
        private void ToggleAllowUnsafeHeaderParsing( bool enable )
        {
            var webConfigSettings = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration( "~" );
            var systemNetConfiguration = webConfigSettings.GetSection( "system.net/settings" ) as System.Net.Configuration.SettingsSection;
            //systemNetConfiguration.HttpWebRequest.UseUnsafeHeaderParsing = enable;
        }

        #endregion utility

        #endregion Temp Methods

        #region Exceptions

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.Exception" />
        public class ReferencePaymentInfoRequired : Exception
        {
            public ReferencePaymentInfoRequired()
                : base( "PiGateway requires a token or customer reference" )
            {
            }
        }

        #endregion 

        #region GatewayComponent implementation

        /// <summary>
        /// Gets the supported payment schedules.
        /// </summary>
        /// <value>
        /// The supported payment schedules.
        /// </value>
        public override List<DefinedValueCache> SupportedPaymentSchedules
        {
            get
            {
                var values = new List<DefinedValueCache>();
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME ) );

                // TODO enable these when Pi add these
                //values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY ) );
                //values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY ) );

                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY ) );
                values.Add( DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY ) );
                return values;
            }
        }

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ReferencePaymentInfoRequired"></exception>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            var customerId = referencedPaymentInfo.ReferenceNumber;

            var response = this.PostTransaction( this.PrivateApiKey, TransactionType.sale, paymentInfo.Amount, customerId );
            if ( !response.Data.IsResponseCodeSuccess() )
            {
                errorMessage = response.Message;
                return null;
            }

            var financialTransaction = new FinancialTransaction();
            financialTransaction.TransactionCode = response.Data.Id;
            return financialTransaction;
        }

        /// <summary>
        /// Credits (Refunds) the specified transaction.
        /// </summary>
        /// <param name="origTransaction">The original transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage )
        {
            if ( origTransaction == null || origTransaction.TransactionCode.IsNullOrWhiteSpace() || origTransaction.FinancialGateway == null )
            {
                errorMessage = "Invalid original transaction, transaction code, or gateway.";
                return null;
            }

            var transactionId = origTransaction.TransactionCode;

            var transactionStatus = this.GetTransactionStatus( this.PrivateApiKey, transactionId );
            var transactionStatusTransaction = transactionStatus.Data.FirstOrDefault( a => a.Id == transactionId );
            TransactionVoidRefundResponse response;
            if ( transactionStatusTransaction.IsPendingSettlement() )
            {
                // https://sandbox.gotnpgateway.com/docs/api/#void
                response = this.PostVoid( this.PrivateApiKey, transactionId );
            }
            else
            {
                https://sandbox.gotnpgateway.com/docs/api/#refund
                response = this.PostRefund( this.PrivateApiKey, transactionId, origTransaction.TotalAmount );
            }

            if ( response.IsSuccessStatus() )
            {
                var transaction = new FinancialTransaction();
                transaction.TransactionCode = "#TODO#";
                errorMessage = string.Empty;
                return transaction;
            }

            errorMessage = response.Message;
            return null;
        }

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="ReferencePaymentInfoRequired"></exception>
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            var referencedPaymentInfo = paymentInfo as ReferencePaymentInfo;
            if ( referencedPaymentInfo == null )
            {
                throw new ReferencePaymentInfoRequired();
            }

            var customerId = referencedPaymentInfo.ReferenceNumber;

            // TODO: Create a new Plan for each subscription, or reuse existing ones??
            CreatePlanParameters createPlanParameters = new CreatePlanParameters
            {
                Name = schedule.TransactionFrequencyValue.Value,
                Description = $"Plan for PersonId: {schedule.PersonId }",
                Amount = paymentInfo.Amount
            };

            SetBillingPlanParameters( createPlanParameters, schedule.TransactionFrequencyValue.Guid );

            var planResponse = this.CreatePlan( this.PrivateApiKey, createPlanParameters );

            var planId = planResponse.Data.Id;

            SubscriptionRequestParameters subscriptionParameters = new SubscriptionRequestParameters
            {
                Customer = new SubscriptionCustomer { Id = customerId },
                PlanId = planId,
                Description = $"Subscription for PersonId: {schedule.PersonId }",
                NextBillDate = schedule.StartDate,
                Duration = 0,
                Amount = paymentInfo.Amount
            };

            SetBillingPlanParameters( subscriptionParameters, schedule.TransactionFrequencyValue.Guid );

            var subscriptionResult = this.CreateSubscription( this.PrivateApiKey, subscriptionParameters );
            var subscriptionId = subscriptionResult.Data?.Id;

            if ( subscriptionId.IsNullOrWhiteSpace() )
            {
                errorMessage = subscriptionResult.Message;
                return null;
            }

            var scheduledTransaction = new FinancialScheduledTransaction();
            scheduledTransaction.TransactionCode = subscriptionId;
            scheduledTransaction.GatewayScheduleId = subscriptionId;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;

            GetScheduledPaymentStatus( scheduledTransaction, out errorMessage );
            return scheduledTransaction;
        }

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool UpdateScheduledPayment( FinancialScheduledTransaction scheduledTransaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            SubscriptionRequestParameters subscriptionParameters = new SubscriptionRequestParameters
            {
                NextBillDate = scheduledTransaction.StartDate,
                Duration = 0,
                Amount = paymentInfo.Amount
            };

            SetBillingPlanParameters( subscriptionParameters, scheduledTransaction.TransactionFrequencyValue.Guid );

            var subscriptionResult = this.UpdateSubscription( this.PrivateApiKey, subscriptionId, subscriptionParameters );
            if ( subscriptionResult.IsSuccessStatus() )
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                errorMessage = subscriptionResult.Message;
                return false;
            }
        }

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool CancelScheduledPayment( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            var subscriptionResult = this.DeleteSubscription( this.PrivateApiKey, subscriptionId );
            if ( subscriptionResult.IsSuccessStatus() )
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                errorMessage = subscriptionResult.Message;
                return false;
            }
        }

        /// <summary>
        /// Flag indicating if gateway supports reactivating a scheduled payment.
        /// </summary>
        public override bool ReactivateScheduledPaymentSupported => false;

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            errorMessage = "The payment gateway associated with this scheduled transaction (Pi) does not support reactivating scheduled transactions. A new scheduled transaction should be created instead.";
            return false;
        }

        /// <summary>
        /// Gets the scheduled payment status.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            var subscriptionId = scheduledTransaction.GatewayScheduleId;

            var subscriptionResult = this.GetSubscription( this.PrivateApiKey, subscriptionId );
            if ( subscriptionResult.IsSuccessStatus() )
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                errorMessage = subscriptionResult.Message;
                return false;
            }
        }

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            QueryTransactionStatusRequest queryTransactionStatusRequest = new QueryTransactionStatusRequest
            {
                DateRange = new QueryDateRange( startDate, endDate )
            };

            var searchResult = this.SearchTransactions( this.PrivateApiKey, queryTransactionStatusRequest );

            if ( !searchResult.IsSuccessStatus() )
            {
                errorMessage = searchResult.Message;
                return null;
            }

            errorMessage = string.Empty;

            var paymentList = new List<Payment>();

            foreach ( var transaction in searchResult.Data )
            {
                var payment = new Payment
                {
                    AccountNumberMasked = transaction.PaymentMethodResponse.Card.MaskedCard,
                    Amount = transaction.Amount,
                    TransactionDateTime = transaction.CreatedDateTime.Value,

                    GatewayScheduleId = transaction.PaymentMethod

                };

                paymentList.Add( payment );
            }


            return paymentList;
        }

        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }


        #endregion GatewayComponent implementation
    }
}
