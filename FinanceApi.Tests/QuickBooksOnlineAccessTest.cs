using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FinanceApi.RequestModels;
using FinanceApi.Routes.Authenticated.PointOfSale;
using FinanceApi.Tests.InfrastructureAsCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;
using Xunit;
using Xunit.Abstractions;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

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
            //Output.WriteLine(JsonConvert.SerializeObject(customers));
        }

        /*
         Reports are nice, but need more work to build out to read granularly.
        private void SetActiveOrInactiveByHavingAnyTransactions(Customer customer, string start, string end)
        {
            var threadSafeClient = new QuickBooksOnlineClient(new XUnitLogger(Output));
            var rawTransactionReport = threadSafeClient.Request($"reports/TransactionList?start_date={start}&end_date={end}&customer={customer.Id}", HttpMethod.Get);
            var transactionReport = JsonConvert.DeserializeObject<TransactionListReport>(rawTransactionReport);
            bool active = transactionReport.Rows?["Row"] != null && transactionReport.Rows["Row"].Any();
            SetCustomer(customer, active);
        }
        */
    }
}
