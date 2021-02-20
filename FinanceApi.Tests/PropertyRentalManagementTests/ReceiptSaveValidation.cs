using NSubstitute;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using Xunit;

namespace FinanceApi.Tests.PropertyRentalManagementTests
{
    public class ReceiptSaveValidation
    {
        [Fact]
        public void Credit_Card_Required_Fields()
        {
            var validation = new ReceiptValidation(Substitute.For<ISpotReservationCheck>()).Validate(
                new Receipt
                {
                    MakeCardPayment = true
                }).Result;
            Assert.Contains("Payment is required and must be at least $0.01.", validation);
            Assert.Contains("Credit card number is required.", validation);
            Assert.Contains("Expiration month is required.", validation);
            Assert.Contains("Expiration year is required.", validation);
            Assert.Contains("CVV is required.", validation);

            var goodValidation = new ReceiptValidation(Substitute.For<ISpotReservationCheck>()).Validate(
                new Receipt
                {
                    ThisPayment = .01m,
                    MakeCardPayment = true,
                    CardPayment = new ReceiptCardPayment
                    {
                        CardNumber = "1234",
                        ExpirationMonth = "12",
                        ExpirationYear = "12",
                        Cvv = "123"
                    }
                }).Result;
            Assert.DoesNotContain("Payment is required and must be at least $5.00.", goodValidation);
            Assert.DoesNotContain("Credit card number is required.", goodValidation);
            Assert.DoesNotContain("Expiration month is required.", goodValidation);
            Assert.DoesNotContain("Expiration year is required.", goodValidation);
            Assert.DoesNotContain("CVV is required.", goodValidation);
        }
    }
}
