using PropertyRentalManagement.DataServices;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode
{
    public class MakeRecurringInvoices
    {
        private ITestOutputHelper Output { get; }

        public MakeRecurringInvoices(ITestOutputHelper output)
        {
            Output = output;
        }

        //[Fact]
        public void MakeMonthlyInvoices()
        {
            var vendorClient = new VendorService();
            var monthlyVendors = vendorClient.GetByPaymentFrequency(Factory.CreateAmazonDynamoDbClient(), "monthly");
            Output.WriteLine(monthlyVendors.Count.ToString());
            foreach (var weeklyVendor in monthlyVendors)
            {
                Output.WriteLine(weeklyVendor.QuickBooksOnlineId.ToString());
            }
        }

    }
}
