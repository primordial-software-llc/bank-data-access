using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using Customer = PropertyRentalManagement.QuickBooksOnline.Models.Customer;
using Invoice = PropertyRentalManagement.QuickBooksOnline.Models.Invoice;

namespace Tests
{
    public class QuickBooksOnlineAccessTest
    {
        private ITestOutputHelper Output { get; }

        public QuickBooksOnlineAccessTest(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void GetCustomerCount()
        {
            var client = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var customers = client.QueryAll<Customer>("select * from customer");
            Output.WriteLine(customers.Count.ToString());
        }

        //[Fact]
        public void Set_Inactive_Customers()
        {
            var client = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var activeCustomers = client.QueryAll<Customer>("select * from Customer Where Active = true");
            var start = new DateTime(2020, 2, 1).ToString("yyyy-MM-dd");
            Parallel.ForEach(activeCustomers, new ParallelOptions { MaxDegreeOfParallelism = 5 }, customer =>
            {
                var payments = client.Query<Payment>($"select * from Payment Where CustomerRef = '{customer.Id}' and TxnDate >= '{start}'");
                if (payments.Count(x => x.TotalAmount > 0) > 0)
                {
                    return;
                }

                var salesReceipts = client.Query<SalesReceipt>($"select * from SalesReceipt Where CustomerRef = '{customer.Id}' and TxnDate >= '{start}'");
                if (salesReceipts.Count > 0)
                {
                    return;
                }

                var invoices = client.Query<Invoice>($"select * from Invoice Where CustomerRef = '{customer.Id}' and TxnDate >= '{start}'");
                if (invoices.Count > 0)
                {
                    return;
                }
                Output.WriteLine("Customer may not be active: " + customer.Id);
                SetCustomer(customer, false);
            });
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

        private void SetCustomer(Customer customer, bool active)
        {
            var jsonUpdate = new JObject
            {
                { "SyncToken", customer.SyncToken },
                { "Id", customer.Id },
                { "Active", active },
                { "sparse", true }
            };
            try
            {
                var client = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
                client.Request("customer", HttpMethod.Post, jsonUpdate.ToString());
                string updateType = active ? "active" : "inactive";
                if (!active)
                {
                    Output.WriteLine($"The customer {customer.DisplayName} with customer id {customer.Id} will be set to {updateType}.");
                }
            }
            catch (Exception)
            {
                Output.WriteLine($"Failed to update customer {customer.DisplayName} with customer id {customer.Id}.");
            }
        }
    }
}
