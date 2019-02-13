using System.Web.UI;
using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// A Financial gateway provider that supports collecting Payment Info (Credit Card Number fields or ACH fields) in the browser.
    /// An IHostedGatewayComponent will return a token in the browser client instead of sending payment info to the Rock Server.
    /// </summary>
    public interface IHostedGatewayComponent
    {
        /// <summary>
        /// Gets the hosted payment information control which will be used to collect CreditCard, ACH fields
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="controlId">The control identifier.</param>
        /// <returns></returns>
        Control GetHostedPaymentInfoControl( FinancialGateway financialGateway, string controlId );

        /// <summary>
        /// Gets the JavaScript needed to tell the hostedPaymentInfoControl to get send the paymentInfo and get a token
        /// Put this on your 'Next' or 'Submit' button so that the hostedPaymentInfoControl will fetch the token/response
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        string GetHostPaymentInfoSubmitScript( FinancialGateway financialGateway, Control hostedPaymentInfoControl );

        /// <summary>
        /// Gets the paymentInfoToken that the hostedPaymentInfoControl returned (see also <seealso cref="M:Rock.Financial.IHostedGatewayComponent.GetHostedPaymentInfoControl(Rock.Model.FinancialGateway,System.String)" />)
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="hostedPaymentInfoControl">The hosted payment information control.</param>
        /// <returns></returns>
        string GetHostedPaymentInfoToken( FinancialGateway financialGateway, Control hostedPaymentInfoControl );
    }
}