using System;
using Api;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class CreateVendorsFromQuickBooksOnlineAndIdentifyPaymentFrequency
    {
        private ITestOutputHelper Output { get; }

        public CreateVendorsFromQuickBooksOnlineAndIdentifyPaymentFrequency(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Run()
        {
            var qboClient = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var awsDbClient = Factory.CreateAmazonDynamoDbClient();

            var activeCustomers = qboClient.QueryAll<Customer>("select * from Customer Where Active = true");
            var start = new DateTime(2020, 2, 1).ToString("yyyy-MM-dd");

            foreach (var customer in activeCustomers)
            {
                var isWeekly = IsWeekly(qboClient, customer.Id, start);
                string paymentFrequency = isWeekly.HasValue && isWeekly.Value ? "weekly" : string.Empty;
                Output.WriteLine($"{customer.Id}: {paymentFrequency}");
                new VendorService().Create(awsDbClient, int.Parse(customer.Id), true, paymentFrequency);
            }
        }

        public bool? IsWeekly(QuickBooksOnlineClient qboClient, string customerId, string date)
        {
            var salesReceipts = qboClient.QueryAll<SalesReceipt>(
                $"select * from SalesReceipt Where TxnDate >= '{date}' and CustomerRef = '{customerId}'");
            var invoices = qboClient.QueryAll<Invoice>(
                $"select * from Invoice Where TxnDate >= '{date}' and CustomerRef = '{customerId}'");

            if (salesReceipts.Count + invoices.Count > 2)
            {
                return true;
            }

            return null;
        }

    }
}
