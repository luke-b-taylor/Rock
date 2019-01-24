using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Financial;
using Rock.Model;

namespace Rock.TransNational
{
    [Description( "TransNational Pi Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "TransNational Pi Gateway" )]
    public class Gateway : GatewayComponent
    {
        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            throw new NotImplementedException();
        }
    }
}
