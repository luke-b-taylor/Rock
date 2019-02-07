using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// from JSON structures on https://sandbox.gotnpgateway.com/docs/api/
/// </summary>
namespace Rock.TransNational.Pi
{
    #region Customer Related

    /// <summary>
    /// https://sandbox.gotnpgateway.com/docs/api/#create-a-new-customer
    /// </summary>
    public class CreateCustomerRequest
    {
        public string description { get; set; }
        public PaymentMethodRequest payment_method { get; set; }
        public BillingAddress billing_address { get; set; }
        public ShippingAddress shipping_address { get; set; }
    }

    /// <summary>
    /// from https://sandbox.gotnpgateway.com/docs/api/#create-a-new-customer
    /// </summary>
    public class CreateCustomerResponse
    {
        [JsonProperty( "status" )]
        public string Status { get; set; }

        [JsonProperty( "msg" )]
        public string Message { get; set; }

        [JsonProperty( "data" )]
        public CreateCustomerResponseData Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CreateCustomerResponseData
    {
        public string id { get; set; }
        public string description { get; set; }
        public PaymentMethodResponse payment_method { get; set; }
        public BillingAddress billing_address { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentMethodRequest
    {
        public PaymentMethodRequest()
        {
        }

        public PaymentMethodRequest( string token )
            : this()
        {
            this.Token = token;
        }

        [Newtonsoft.Json.JsonProperty( "token" )]
        public string Token { get; set; }

        [Newtonsoft.Json.JsonProperty( "Customer" )]
        public PaymentMethodCustomer Customer { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentMethodCustomer
    {
        public string id { get; set; }
        public string payment_method_type { get; set; }
        public string payment_method_id { get; set; }
        public string billing_address_id { get; set; }
        public string shipping_address_id { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentMethodResponse
    {
        [Newtonsoft.Json.JsonProperty( "card" )]
        public CardInfoResponse Card { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CardInfoResponse
    {
        public string id { get; set; }
        public string card_type { get; set; }
        public string first_six { get; set; }
        public string last_four { get; set; }
        public string masked_card { get; set; }
        public string expiration_date { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    #endregion Customer Related

    #region shared types

    /// <summary>
    /// 
    /// </summary>
    public class BillingAddress
    {
        public string id { get; set; }
        public string customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string company { get; set; }
        public string address_line_1 { get; set; }
        public string address_line_2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string fax { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.TransNational.Pi.BillingAddress" />
    public class ShippingAddress : BillingAddress
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class BillingPlanParameters
    {
        /// <summary>
        /// "How often to run the billing cycle. Run every x months"
        /// </summary>
        /// <value>
        /// The billing cycle interval.
        /// </value>
        [JsonProperty( "billing_cycle_interval" )]
        public int? BillingCycleInterval { get; set; }

        /// <summary>
        /// "How often run within a billing cycle. (monthly..."
        /// </summary>
        /// <value>
        /// The billing frequency.
        /// </value>
        [JsonProperty( "billing_frequency" )]
        [JsonConverter( typeof( StringEnumConverter ) )]
        public BillingFrequency? BillingFrequency { get; set; }

        /// <summary>
        /// "Which day to bill on. If twice_monthly, then comma separate dates"
        /// </summary>
        /// <value>
        /// The billing days.
        /// </value>
        [JsonProperty( "billing_days" )]
        public string BillingDays { get; set; }

        /// <summary>
        /// Gets or sets the duration (??)
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        [JsonProperty( "duration" )]
        public int? Duration { get; set; }

        /// <summary>
        /// Gets or sets the amount in Dollars (and sets <seealso cref="AmountCents"/>)
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [JsonIgnore]
        public decimal Amount
        {
            get => AmountCents / 100;
            set => AmountCents = ( int ) ( value * 100 );
        }

        /// <summary>
        /// Gets or sets the "amount to be discounted" (Payment Amount) (in cents)
        /// </summary>
        /// <value>
        /// The amount cents.
        /// </value>
        [JsonProperty( "amount" )]
        public int AmountCents { get; set; }
    }

    #endregion shared types

    #region Transactions 

    public class CreateTransaction
    {
        public string type { get; set; }

        /// <summary>
        /// Gets or sets the amount (in cents)
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public int amount { get; set; }

        public int tax_amount { get; set; }
        public int shipping_amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string order_id { get; set; }
        public string po_number { get; set; }
        public string ip_address { get; set; }
        public bool email_receipt { get; set; }
        public string email_address { get; set; }
        public bool create_vault_record { get; set; }
        public PaymentMethodRequest payment_method { get; set; }
        public BillingAddress billing_address { get; set; }
        public ShippingAddress shipping_address { get; set; }
    }


    public class CreateTransactionResponse
    {
        public string status { get; set; }
        public string msg { get; set; }
        public TransactionResponseData data { get; set; }
    }

    public class TransactionResponseData
    {
        public string id { get; set; }
        public string type { get; set; }
        public int amount { get; set; }
        public int tax_amount { get; set; }
        public bool tax_exempt { get; set; }
        public int shipping_amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string order_id { get; set; }
        public string po_number { get; set; }
        public string ip_address { get; set; }
        public bool email_receipt { get; set; }
        public string email_address { get; set; }
        public string payment_method { get; set; }

        // NOTE: this is documented as just 'response', but it is actually response_body (when using a token at least)
        public TransactionPaymentInfo response_body { get; set; }

        public string status { get; set; }
        public int response_code { get; set; }
        public string customer_id { get; set; }
        public BillingAddress billing_address { get; set; }
        public ShippingAddress shipping_address { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class TransactionPaymentInfo
    {
        public PaymentInfoCardResponse card { get; set; }
    }

    public class PaymentInfoCardResponse
    {
        public string id { get; set; }
        public string card_type { get; set; }
        public string first_six { get; set; }
        public string last_four { get; set; }
        public string masked_card { get; set; }
        public string expiration_date { get; set; }
        public string status { get; set; }
        public string auth_code { get; set; }
        public string processor_response_code { get; set; }
        public string processor_response_text { get; set; }
        public string processor_type { get; set; }
        public string processor_id { get; set; }
        public string avs_response_code { get; set; }
        public string cvv_response_code { get; set; }
        public Processor_Specific processor_specific { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Processor_Specific
    {
    }

    #endregion Transactions

    #region Plans

    /// <summary>
    /// https://sandbox.gotnpgateway.com/docs/api/#create-a-plan
    /// </summary>
    public class CreatePlanParameters : BillingPlanParameters
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty( "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty( "description" )]
        public string Description { get; set; }


    }

    public class CreatePlanResponse
    {
        public string status { get; set; }
        public string msg { get; set; }

        [JsonProperty( "data" )]
        public PlanData Data { get; set; }
    }

    public class PlanData
    {
        /// <summary>
        /// Gets or sets the Plan Id
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public int amount { get; set; }
        public int billing_cycle_interval { get; set; }
        public string billing_frequency { get; set; }
        public string billing_days { get; set; }
        public int duration { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    /// <summary>
    /// Result from GetPlans
    /// https://sandbox.gotnpgateway.com/docs/api/#get-all-plans
    /// </summary>
    public class GetPlansResult
    {
        public string status { get; set; }
        public string msg { get; set; }
        public int total_count { get; set; }

        [JsonProperty( "data" )]
        public PlanData[] Data;
    }

    #endregion Plan

    #region Subscriptions


    /// <summary>
    /// https://sandbox.gotnpgateway.com/docs/api/#create-a-subscription
    /// </summary>
    public class CreateSubscriptionParameters : BillingPlanParameters
    {
        [JsonProperty( "plan_id" )]
        public string PlanId { get; set; }

        [JsonProperty( "description" )]
        public string Description { get; set; }

        [JsonProperty( "customer" )]
        public SubscriptionCustomer Customer { get; set; }

        [JsonProperty( "next_bill_date" )]
        [JsonConverter( typeof( RockJsonIsoDateConverter ) )]
        public DateTime NextBillDate { get; set; }

        //public object[] add_ons { get; set; }
        //public Discount[] discounts { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SubscriptionCustomer
    {
        /// <summary>
        /// Gets or sets the customer id
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }
    }

    /// <summary>
    /// https://sandbox.gotnpgateway.com/docs/api/#create-a-subscription
    /// </summary>
    public class CreateSubscriptionResponse
    {
        public string status { get; set; }
        public string msg { get; set; }

        [JsonProperty( "data" )]
        public SubscriptionData Data { get; set; }
    }

    /// <summary>
    /// https://sandbox.gotnpgateway.com/docs/api/#create-a-subscription
    /// </summary>
    public class SubscriptionData
    {

        /// <summary>
        /// Gets or sets the Subscription Id
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public int amount { get; set; }
        public int billing_cycle_interval { get; set; }
        public string billing_frequency { get; set; }
        public string billing_days { get; set; }
        public int duration { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    #endregion Subscriptions

    #region Rock Wrapper Types

    /// <summary>
    /// ToDo: Move this to Rock.Utility
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Converters.IsoDateTimeConverter" />
    public class RockJsonIsoDateConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public RockJsonIsoDateConverter()
        {
            this.DateTimeFormat = "yyyy-MM-dd";
        }

    }

    /// <summary>
    /// ToDo: This list might not be complete
    /// </summary>
    public enum BillingFrequency
    {
        monthly,
        twice_monthly
    }

    #endregion Rock Wrapper Types
}