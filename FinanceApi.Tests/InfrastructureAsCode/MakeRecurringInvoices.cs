using Api;
using FinanceApi.Tests.InfrastructureAsCode;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class MakeRecurringInvoices
    {
        private ITestOutputHelper Output { get; }

        public MakeRecurringInvoices(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void MakeWeeklyInvoices()
        {
            var vendorClient = new VendorService();
            var weeklyVendors = vendorClient.GetByPaymentFrequency(Factory.CreateAmazonDynamoDbClient(), "weekly");
            Output.WriteLine(weeklyVendors.Count.ToString());
            foreach (var weeklyVendor in weeklyVendors)
            {
                Output.WriteLine(weeklyVendor.QuickBooksOnlineId.ToString());
            }
        }


    }
}
