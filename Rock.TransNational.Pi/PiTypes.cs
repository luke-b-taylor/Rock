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
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty( "description" )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        /// <value>
        /// The payment method.
        /// </value>
        [JsonProperty( "payment_method" )]
        public PaymentMethodRequest PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// </summary>
        /// <value>
        /// The billing address.
        /// </value>
        [JsonProperty( "billing_address" )]
        public BillingAddress BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the shipping address.
        /// </summary>
        /// <value>
        /// The shipping address.
        /// </value>
        [JsonProperty( "shipping_address" )]
        public ShippingAddress ShippingAddress { get; set; }
    }

    /// <summary>
    /// from https://sandbox.gotnpgateway.com/docs/api/#create-a-new-customer
    /// </summary>
    public class CreateCustomerResponse
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty( "msg" )]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty( "data" )]
        public CreateCustomerResponseData Data { get; set; }
    }

    /// <summary>
    /// from https://sandbox.gotnpgateway.com/docs/api/#create-a-new-customer
    /// </summary>
    public class CreateCustomerResponseData
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty( "description" )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        /// <value>
        /// The payment method.
        /// </value>
        [JsonProperty( "payment_method" )]
        public PaymentMethodResponse PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// </summary>
        /// <value>
        /// The billing address.
        /// </value>
        [JsonProperty( "billing_address" )]
        public BillingAddress BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [JsonProperty( "created_at" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the update date time.
        /// </summary>
        /// <value>
        /// The update date time.
        /// </value>
        [JsonProperty( "updated_at" )]
        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentMethodRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentMethodRequest"/> class.
        /// </summary>
        /// <param name="customer">The customer.</param>
        public PaymentMethodRequest( PaymentMethodCustomer customer )
        {
            this.Customer = customer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentMethodRequest"/> class.
        /// </summary>
        /// <param name="token">The token.</param>
        public PaymentMethodRequest( string token )
        {
            this.Token = token;
        }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        [Newtonsoft.Json.JsonProperty( "token" )]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        /// <value>
        /// The customer.
        /// </value>
        [Newtonsoft.Json.JsonProperty( "customer" )]
        public PaymentMethodCustomer Customer { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentMethodCustomer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentMethodCustomer"/> class.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        public PaymentMethodCustomer( string customerId )
        {
            Id = customerId;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "Id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the payment method.
        /// </summary>
        /// <value>
        /// The type of the payment method.
        /// </value>
        [JsonProperty( "payment_method_type" )]
        public string PaymentMethodType { get; set; }

        /// <summary>
        /// Gets or sets the payment method identifier.
        /// </summary>
        /// <value>
        /// The payment method identifier.
        /// </value>
        [JsonProperty( "payment_method_id" )]
        public string PaymentMethodId { get; set; }

        /// <summary>
        /// Gets or sets the billing address identifier.
        /// </summary>
        /// <value>
        /// The billing address identifier.
        /// </value>
        [JsonProperty( "billing_address_id" )]
        public string BillingAddressId { get; set; }

        /// <summary>
        /// Gets or sets the shipping address identifier.
        /// </summary>
        /// <value>
        /// The shipping address identifier.
        /// </value>
        [JsonProperty( "shipping_address_id" )]
        public string ShippingAddressId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentMethodResponse
    {
        /// <summary>
        /// Gets or sets the card.
        /// </summary>
        /// <value>
        /// The card.
        /// </value>
        [Newtonsoft.Json.JsonProperty( "card" )]
        public CardInfoResponse Card { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CardInfoResponse
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the card.
        /// </summary>
        /// <value>
        /// The type of the card.
        /// </value>
        [JsonProperty( "card_type" )]
        public string CardType { get; set; }

        /// <summary>
        /// Gets or sets the first six.
        /// </summary>
        /// <value>
        /// The first six.
        /// </value>
        [JsonProperty( "first_six" )]
        public string FirstSix { get; set; }

        /// <summary>
        /// Gets or sets the last four.
        /// </summary>
        /// <value>
        /// The last four.
        /// </value>
        [JsonProperty( "last_four" )]
        public string LastFour { get; set; }

        /// <summary>
        /// Gets or sets the masked card.
        /// </summary>
        /// <value>
        /// The masked card.
        /// </value>
        [JsonProperty( "masked_card" )]
        public string MaskedCard { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        /// <value>
        /// The expiration date.
        /// </value>
        [JsonProperty( "expiration_date" )]
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [JsonProperty( "created_at" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the updated date time.
        /// </summary>
        /// <value>
        /// The updated date time.
        /// </value>
        [JsonProperty( "updated_at" )]
        public DateTime? UpdatedDateTime { get; set; }
    }

    #endregion Customer Related

    #region shared types

    /// <summary>
    /// 
    /// </summary>
    public class BillingAddress
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        [JsonProperty( "customer_id" )]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [JsonProperty( "first_name" )]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [JsonProperty( "last_name" )]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        [JsonProperty( "company" )]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the address line 1.
        /// </summary>
        /// <value>
        /// The address line 1.
        /// </value>
        [JsonProperty( "address_line_1" )]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Gets or sets the address line2.
        /// </summary>
        /// <value>
        /// The address line2.
        /// </value>
        [JsonProperty( "address_line_2" )]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [JsonProperty( "city" )]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [JsonProperty( "state" )]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [JsonProperty( "postal_code" )]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [JsonProperty( "country" )]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [JsonProperty( "email" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        [JsonProperty( "phone" )]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the fax.
        /// </summary>
        /// <value>
        /// The fax.
        /// </value>
        [JsonProperty( "fax" )]
        public string Fax { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [JsonProperty( "created_at" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the updated date time.
        /// </summary>
        /// <value>
        /// The updated date time.
        /// </value>
        [JsonProperty( "updated_at" )]
        public DateTime? UpdatedDateTime { get; set; }
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
        /// Gets or sets the amount (in cents)
        /// </summary>
        /// <value>
        /// The amount cents.
        /// </value>
        [JsonProperty( "amount" )]
        public int AmountCents { get; set; }
    }

    #endregion shared types

    #region Transactions 

    /// <summary>
    /// https://sandbox.gotnpgateway.com/docs/api/#processing-a-transaction
    /// </summary>
    public class CreateTransaction
    {
        /// <summary>
        /// sale, authorize, credit
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonProperty( "type" )]
        public string Type { get; set; }

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
        /// Gets or sets the processed amount (in cents)
        /// </summary>
        /// <value>
        /// The amount cents.
        /// </value>
        [JsonProperty( "amount" )]
        public int AmountCents { get; set; }

        /// <summary>
        /// Gets or sets the tax amount (in cents).
        /// </summary>
        /// <value>
        /// The tax amount.
        /// </value>
        [JsonProperty( "tax_amount" )]
        public int tax_amount { get; set; }

        /// <summary>
        /// Gets or sets the shipping amount (in cents).
        /// </summary>
        /// <value>
        /// The shipping amount.
        /// </value>
        [JsonProperty( "shipping_amount" )]
        public int shipping_amount { get; set; }

        /// <summary>
        /// ISO 4217 currency. Ex USD
        /// </summary>
        /// <value>
        /// The currency.
        /// </value>
        [JsonProperty( "currency" )]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the description (max length 255)
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty( "description" )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [JsonProperty( "order_id" )]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the po number.
        /// </summary>
        /// <value>
        /// The po number.
        /// </value>
        [JsonProperty( "po_number" )]
        public string po_number { get; set; }

        /// <summary>
        /// IPv4 or IPv6 value of the end user
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        [JsonProperty( "ip_address" )]
        public string IPAddress { get; set; }

        /// <summary>
        /// Bool value to trigger sending of an email receipt if an email was provided.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [email receipt]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty( "email_receipt" )]
        public bool EmailReceipt { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        [JsonProperty( "email_address" )]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Bool value to trigger the creation of a customer vault record, if the transaction is successful
        /// </summary>
        /// <value>
        ///   <c>true</c> if [create vault record]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty( "create_vault_record" )]
        public bool CreateVaultRecord { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        /// <value>
        /// The payment method.
        /// </value>
        [JsonProperty( "payment_method" )]
        public PaymentMethodRequest PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// </summary>
        /// <value>
        /// The billing address.
        /// </value>
        [JsonProperty( "billing_address" )]
        public BillingAddress BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the shipping address.
        /// </summary>
        /// <value>
        /// The shipping address.
        /// </value>
        [JsonProperty( "shipping_address" )]
        public ShippingAddress ShippingAddress { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CreateTransactionResponse
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty( "msg" )]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty( "data" )]
        public TransactionResponseData Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TransactionResponseData
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonProperty( "type" )]
        public string Type { get; set; }

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
        /// Gets or sets the amount (in cents)
        /// </summary>
        /// <value>
        /// The amount cents.
        /// </value>
        [JsonProperty( "amount" )]
        public int AmountCents { get; set; }

        /// <summary>
        /// Gets or sets the tax amount (in cents).
        /// </summary>
        /// <value>
        /// The tax amount.
        /// </value>
        [JsonProperty( "tax_amount" )]
        public int TaxAmount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [tax exempt].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tax exempt]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty( "tax_exempt" )]
        public bool TaxExempt { get; set; }

        /// <summary>
        /// Gets or sets the shipping amount (in cents). 
        /// </summary>
        /// <value>
        /// The shipping amount.
        /// </value>
        [JsonProperty( "shipping_amount" )]
        public int ShippingAmount { get; set; }

        /// <summary>
        /// ISO 4217 currency. Ex USD
        /// </summary>
        /// <value>
        /// The currency.
        /// </value>
        [JsonProperty( "currency" )]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty( "description" )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [JsonProperty( "order_id" )]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the po number.
        /// </summary>
        /// <value>
        /// The po number.
        /// </value>
        [JsonProperty( "po_number" )]
        public string PONumber { get; set; }

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        [JsonProperty( "ip_address" )]
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [email receipt].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [email receipt]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty( "email_receipt" )]
        public bool EmailReceipt { get; set; }

        /// <summary>
        /// Gets or sets the email address
        /// <value>
        /// The email address.
        /// </value>
        [JsonProperty( "email_address" )]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        /// <value>
        /// The payment method.
        /// </value>
        [JsonProperty( "payment_method" )]
        public string PaymentMethod { get; set; }

        /// NOTE: this is documented as just 'response', but it is actually response_body (when using a token at least)
        /// <summary>
        /// Gets or sets the response body.
        /// </summary>
        /// <value>
        /// The response body.
        /// </value>
        [JsonProperty( "response_body" )]
        public TransactionPaymentInfo ResponseBody { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// see https://sandbox.gotnpgateway.com/docs/api/#response-codes
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [JsonProperty( "response_code" )]
        public int ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        [JsonProperty( "customer_id" )]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// </summary>
        /// <value>
        /// The billing address.
        /// </value>
        [JsonProperty( "billing_address" )]
        public BillingAddress BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the shipping address.
        /// </summary>
        /// <value>
        /// The shipping address.
        /// </value>
        [JsonProperty( "shipping_address" )]
        public ShippingAddress ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [JsonProperty( "created_at" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the updated date time.
        /// </summary>
        /// <value>
        /// The updated date time.
        /// </value>
        [JsonProperty( "updated_at" )]
        public DateTime? UpdatedDateTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TransactionPaymentInfo
    {
        /// <summary>
        /// Gets or sets the card.
        /// </summary>
        /// <value>
        /// The card.
        /// </value>
        [JsonProperty( "card" )]
        public PaymentInfoCardResponse Card { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PaymentInfoCardResponse
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the card.
        /// </summary>
        /// <value>
        /// The type of the card.
        /// </value>
        [JsonProperty( "card_type" )]
        public string CardType { get; set; }

        /// <summary>
        /// Gets or sets the first six.
        /// </summary>
        /// <value>
        /// The first six.
        /// </value>
        [JsonProperty( "first_six" )]
        public string FirstSix { get; set; }

        /// <summary>
        /// Gets or sets the last four.
        /// </summary>
        /// <value>
        /// The last four.
        /// </value>
        [JsonProperty( "last_four" )]
        public string LastFour { get; set; }

        /// <summary>
        /// Gets or sets the masked card.
        /// </summary>
        /// <value>
        /// The masked card.
        /// </value>
        [JsonProperty( "masked_card" )]
        public string MaskedCard { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        /// <value>
        /// The expiration date.
        /// </value>
        [JsonProperty( "expiration_date" )]
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the authentication code.
        /// </summary>
        /// <value>
        /// The authentication code.
        /// </value>
        [JsonProperty( "auth_code" )]
        public string AuthCode { get; set; }

        /// <summary>
        /// Gets or sets the processor response code.
        /// </summary>
        /// <value>
        /// The processor response code.
        /// </value>
        [JsonProperty( "processor_response_code" )]
        public string ProcessorResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the processor response text.
        /// </summary>
        /// <value>
        /// The processor response text.
        /// </value>
        [JsonProperty( "processor_response_text" )]
        public string ProcessorResponseText { get; set; }

        /// <summary>
        /// Gets or sets the type of the processor.
        /// </summary>
        /// <value>
        /// The type of the processor.
        /// </value>
        [JsonProperty( "processor_type" )]
        public string ProcessorType { get; set; }

        /// <summary>
        /// Gets or sets the processor identifier.
        /// </summary>
        /// <value>
        /// The processor identifier.
        /// </value>
        [JsonProperty( "processor_id" )]
        public string ProcessorId { get; set; }

        /// <summary>
        /// Gets or sets the avs response code.
        /// </summary>
        /// <value>
        /// The avs response code.
        /// </value>
        [JsonProperty( "avs_response_code" )]
        public string AVSResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the CVV response code.
        /// </summary>
        /// <value>
        /// The CVV response code.
        /// </value>
        [JsonProperty( "cvv_response_code" )]
        public string CVVResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the processor specific.
        /// </summary>
        /// <value>
        /// The processor specific.
        /// </value>
        [JsonProperty( "processor_specific" )]
        public Processor_Specific ProcessorSpecific { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [JsonProperty( "created_at" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the updated date time.
        /// </summary>
        /// <value>
        /// The updated date time.
        /// </value>
        [JsonProperty( "updated_at" )]
        public DateTime? UpdatedDateTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    public class CreatePlanResponse
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty( "msg" )]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty( "data" )]
        public PlanData Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
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
        /// Gets or sets the amount (in cents)
        /// </summary>
        /// <value>
        /// The amount cents.
        /// </value>
        [JsonProperty( "amount" )]
        public int AmountCents { get; set; }

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
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        [JsonProperty( "duration" )]
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [JsonProperty( "created_at" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the updated date time.
        /// </summary>
        /// <value>
        /// The updated date time.
        /// </value>
        [JsonProperty( "updated_at" )]
        public DateTime? UpdatedDateTime { get; set; }
    }

    /// <summary>
    /// Result from GetPlans
    /// https://sandbox.gotnpgateway.com/docs/api/#get-all-plans
    /// </summary>
    public class GetPlansResult
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty( "msg" )]
        public string Message { get; set; }

        /// <summary>
        /// The data
        /// </summary>
        [JsonProperty( "data" )]
        public PlanData[] Data { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        [JsonProperty( "total_count" )]
        public int TotalCount { get; set; }
    }

    #endregion Plan

    #region Subscriptions

    /// <summary>
    /// https://sandbox.gotnpgateway.com/docs/api/#create-a-subscription
    /// </summary>
    public class CreateSubscriptionParameters : BillingPlanParameters
    {
        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>
        /// The plan identifier.
        /// </value>
        [JsonProperty( "plan_id" )]
        public string PlanId { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty( "description" )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        /// <value>
        /// The customer.
        /// </value>
        [JsonProperty( "customer" )]
        public SubscriptionCustomer Customer { get; set; }

        /// <summary>
        /// Gets or sets the next bill date.
        /// </summary>
        /// <value>
        /// The next bill date.
        /// </value>
        [JsonProperty( "next_bill_date" )]
        [JsonConverter( typeof( RockJsonIsoDateConverter ) )]
        public DateTime? NextBillDate { get; set; }
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
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty( "msg" )]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
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
        /// Gets or sets the amount (in cents)
        /// </summary>
        /// <value>
        /// The amount cents.
        /// </value>
        [JsonProperty( "amount" )]
        public int AmountCents { get; set; }

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
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [JsonProperty( "created_at" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the updated date time.
        /// </summary>
        /// <value>
        /// The updated date time.
        /// </value>
        [JsonProperty( "updated_at" )]
        public DateTime? UpdatedDateTime { get; set; }
    }

    #endregion Subscriptions

    #region Query Transaction Status

    /// <summary>
    /// 
    /// </summary>
    public class QueryTransactionStatusRequest
    {
        /// <summary>
        /// Gets or sets the transaction identifier search (optional)
        /// </summary>
        /// <value>
        /// The transaction identifier search.
        /// </value>
        [JsonProperty( "transaction_id", NullValueHandling = NullValueHandling.Ignore )]
        public QuerySearchString TransactionIdSearch { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier search (optional)
        /// </summary>
        /// <value>
        /// The customer identifier search.
        /// </value>
        [JsonProperty( "customer_id", NullValueHandling = NullValueHandling.Ignore )]
        public QuerySearchString CustomerIdSearch { get; set; }

        /// <summary>
        /// Gets or sets the search amount in cents (optional).
        /// </summary>
        /// <value>
        /// The search amount.
        /// </value>
        [JsonProperty( "amount", NullValueHandling = NullValueHandling.Ignore )]
        public QuerySearchInt SearchAmount { get; set; }

        /// <summary>
        /// Gets or sets the date range (optional).
        /// </summary>
        /// <value>
        /// The date range.
        /// </value>
        [JsonProperty( "created_at", NullValueHandling = NullValueHandling.Ignore )]
        public QuerySearchInt DateRange { get; set; }

        /// <summary>
        /// Maximum records to return (0-100, optional)
        /// https://sandbox.gotnpgateway.com/docs/api/#query-transactions
        /// </summary>
        /// <value>
        /// The limit.
        /// </value>
        [JsonProperty( "limit", NullValueHandling = NullValueHandling.Ignore )]
        public int? Limit { get; set; }

        /// <summary>
        /// Number of records to offset the return by (optional)
        /// https://sandbox.gotnpgateway.com/docs/api/#query-transactions
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        [JsonProperty( "offset", NullValueHandling = NullValueHandling.Ignore )]
        public int? Offset { get; set; }
    }

    /// <summary>
    /// see https://sandbox.gotnpgateway.com/docs/api/#query-transactions
    /// </summary>
    public class QuerySearchString
    {
        /// <summary>
        /// Gets or sets the comparison operator.
        /// Possible values are "=, !="
        /// </summary>
        /// <value>
        /// The comparison operator.
        /// </value>
        [JsonProperty( "operator" )]
        public string ComparisonOperator { get; set; }

        /// <summary>
        /// Gets or sets the search value.
        /// </summary>
        /// <value>
        /// The search value.
        /// </value>
        [JsonProperty( "value" )]
        public string SearchValue { get; set; }
    }

    /// <summary>
    /// see https://sandbox.gotnpgateway.com/docs/api/#query-transactions
    /// </summary>
    public class QuerySearchInt
    {
        /// <summary>
        /// Gets or sets the operator.
        /// Possible values are "=, !=, &lt;, &gt;".
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        [JsonProperty( "operator" )]
        public string ComparisonOperator { get; set; }

        /// <summary>
        /// Gets or sets the search value.
        /// </summary>
        /// <value>
        /// The search value.
        /// </value>
        [JsonProperty( "value" )]
        public int SearchValue { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class QueryDateRange
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [JsonConverter( typeof( RockJsonIsoDateConverter ) )]
        [JsonProperty( "start_date" )]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [JsonConverter( typeof( RockJsonIsoDateConverter ) )]
        [JsonProperty( "end_date" )]
        public DateTime? EndDate { get; set; }
    }

    #endregion

    #region Transaction Query Response

    /// <summary>
    /// 
    /// </summary>
    public class TransactionQueryResult
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty( "msg" )]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        [JsonProperty( "total_count" )]
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty( "data" )]
        public TransactionQueryResultData[] Data { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TransactionQueryResultData
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty( "id" )]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        [JsonProperty( "user_id" )]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the idempotency key.
        /// </summary>
        /// <value>
        /// The idempotency key.
        /// </value>
        [JsonProperty( "idempotency_key" )]
        public string IdempotencyKey { get; set; }

        /// <summary>
        /// Gets or sets the idempotency time.
        /// </summary>
        /// <value>
        /// The idempotency time.
        /// </value>
        [JsonProperty( "idempotency_time" )]
        public int IdempotencyTime { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonProperty( "type" )]
        public string Type { get; set; }

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
        /// Gets or sets the amount (in cents)
        /// </summary>
        /// <value>
        /// The amount cents.
        /// </value>
        [JsonProperty( "amount" )]
        public int AmountCents { get; set; }

        /// <summary>
        /// Gets or sets the amount authorized (in cents).
        /// </summary>
        /// <value>
        /// The amount authorized.
        /// </value>
        [JsonProperty( "amount_authorized" )]
        public int AmountAuthorized { get; set; }

        /// <summary>
        /// Gets or sets the amount captured (in cents).
        /// </summary>
        /// <value>
        /// The amount captured.
        /// </value>
        [JsonProperty( "amount_captured" )]
        public int AmountCaptured { get; set; }

        /// <summary>
        /// Gets or sets the amount settled (in cents).
        /// </summary>
        /// <value>
        /// The amount settled.
        /// </value>
        [JsonProperty( "amount_settled" )]
        public int AmountSettled { get; set; }

        /// <summary>
        /// Gets or sets the processor identifier.
        /// </summary>
        /// <value>
        /// The processor identifier.
        /// </value>
        [JsonProperty( "processor_id" )]
        public string ProcessorId { get; set; }

        /// <summary>
        /// Gets or sets the type of the processor.
        /// </summary>
        /// <value>
        /// The type of the processor.
        /// </value>
        [JsonProperty( "processor_type" )]
        public string ProcessorType { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        /// <value>
        /// The payment method.
        /// </value>
        [JsonProperty( "payment_method" )]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the type of the payment.
        /// </summary>
        /// <value>
        /// The type of the payment.
        /// </value>
        [JsonProperty( "payment_type" )]
        public string PaymentType { get; set; }

        /// <summary>
        /// Gets or sets the tax amount (in cents).
        /// </summary>
        /// <value>
        /// The tax amount.
        /// </value>
        [JsonProperty( "tax_amount" )]
        public int TaxAmount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [tax exempt].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tax exempt]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty( "tax_exempt" )]
        public bool TaxExempt { get; set; }

        /// <summary>
        /// Gets or sets the shipping amount (in cents)
        /// </summary>
        /// <value>
        /// The shipping amount.
        /// </value>
        [JsonProperty( "shipping_amount" )]
        public int ShippingAmount { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>
        /// The currency.
        /// </value>
        [JsonProperty( "currency" )]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty( "description" )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        /// <value>
        /// The order identifier.
        /// </value>
        [JsonProperty( "order_id" )]
        public string OrderId { get; set; }

        /// <summary>
        /// Gets or sets the po number.
        /// </summary>
        /// <value>
        /// The po number.
        /// </value>
        [JsonProperty( "po_number" )]
        public string PONumber { get; set; }

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        [JsonProperty( "ip_address" )]
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the transaction source.
        /// </summary>
        /// <value>
        /// The transaction source.
        /// </value>
        [JsonProperty( "transaction_source" )]
        public string TransactionSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [email receipt].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [email receipt]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty( "email_receipt" )]
        public bool EmailReceipt { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>
        /// The customer identifier.
        /// </value>
        [JsonProperty( "customer_id" )]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the referenced transaction identifier.
        /// </summary>
        /// <value>
        /// The referenced transaction identifier.
        /// </value>
        [JsonProperty( "referenced_transaction_id" )]
        public string ReferencedTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the response body.
        /// </summary>
        /// <value>
        /// The response body.
        /// </value>
        [JsonProperty( "response_body" )]
        public object ResponseBody { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        [JsonProperty( "response" )]
        public string Response { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// https://sandbox.gotnpgateway.com/docs/api/#response-codes
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [JsonProperty( "response_code" )]
        public int ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// </summary>
        /// <value>
        /// The billing address.
        /// </value>
        [JsonProperty( "billing_address" )]
        public BillingAddress BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the shipping address.
        /// </summary>
        /// <value>
        /// The shipping address.
        /// </value>
        [JsonProperty( "shipping_address" )]
        public ShippingAddress ShippingAddress { get; set; }

        /// <summary>
        /// Searches by created_at between the provided start_date and end_date. Dates in UTC "YYYY-MM-DDTHH:II:SSZ"
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [JsonProperty( "created_at" )]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the updated date time.
        /// </summary>
        /// <value>
        /// The updated date time.
        /// </value>
        [JsonProperty( "updated_at" )]
        public DateTime? UpdatedDateTime { get; set; }

        /// <summary>
        /// Searches by captured_at between the provided start_date and end_date. Dates in UTC "YYYY-MM-DDTHH:II:SSZ"
        /// </summary>
        /// <value>
        /// The captured date time.
        /// </value>
        [JsonProperty( "captured_at" )]
        public DateTime? CapturedDateTime { get; set; }

        /// <summary>
        /// Searches by settled_at between the provided start_date and end_date. Dates in UTC "YYYY-MM-DDTHH:II:SSZ"
        /// </summary>
        /// <value>
        /// The settled date time.
        /// </value>
        [JsonProperty( "settled_at" )]
        public DateTime? SettledDateTime { get; set; }
    }

    #endregion

    #region Rock Wrapper Types

    /// <summary>
    /// ToDo: Move this to Rock.Utility
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Converters.IsoDateTimeConverter" />
    public class RockJsonIsoDateConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockJsonIsoDateConverter"/> class.
        /// </summary>
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