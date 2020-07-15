using PropertyRentalManagement.BusinessLogic;
using Xunit;

namespace FinanceApi.Tests
{
    public class PaymentApplicatorTests
    {
        [Fact]
        public void ExactPayment()
        {
            Assert.Equal(10, PaymentApplicator.GetPayment(10, 10));
        }

        [Fact]
        public void UnderPayment()
        {
            Assert.Equal(5, PaymentApplicator.GetPayment(5, 10));
        }

        [Fact]
        public void OverPayment()
        {
            Assert.Equal(0, PaymentApplicator.GetPayment(20, 0));
            Assert.Equal(9, PaymentApplicator.GetPayment(20, 9));
            Assert.Equal(10, PaymentApplicator.GetPayment(20, 10));
            Assert.Equal(11, PaymentApplicator.GetPayment(20, 11));
        }

        [Fact]
        public void NoInvoice()
        {
            Assert.Equal(10, PaymentApplicator.GetPayment(10, null));
        }
    }
}
