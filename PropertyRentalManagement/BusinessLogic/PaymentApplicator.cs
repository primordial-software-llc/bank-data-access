
namespace PropertyRentalManagement.BusinessLogic
{
    public class PaymentApplicator
    {
        public static decimal GetPayment(decimal payment, decimal invoiceAmount)
        {
            if (payment <= invoiceAmount)
            {
                return payment;
            }
            else
            {
                return payment - invoiceAmount;
            }
        }
    }
}
