using FinanceApi.Tests.InfrastructureAsCode;
using Newtonsoft.Json;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
{
    public class QuickBooksOnlineAccessTest
    {
        private ITestOutputHelper Output { get; }

        public QuickBooksOnlineAccessTest(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void GetCustomers()
        {
            var qboClient = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var customers = qboClient.QueryAll<Customer>("select * from Customer");
            Output.WriteLine(customers.Count.ToString());
            Output.WriteLine(JsonConvert.SerializeObject(customers));
        }

    }
}
