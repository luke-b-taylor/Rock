using System;

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
        [Newtonsoft.Json.JsonProperty( "status" )]
        public string Status { get; set; }

        [Newtonsoft.Json.JsonProperty( "msg" )]
        public string Message { get; set; }

        [Newtonsoft.Json.JsonProperty( "data" )]
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
        public TransactionPaymentInfo response { get; set; }
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
}