using System;
using PropertyRentalManagement.DataServices;
using System.Linq;
using Api;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode.PointOfSale
{
    public class CreateVendorsFromQuickBooksOnlineAndIdentifyPaymentFrequency
    {
        private ITestOutputHelper Output { get; }

        public CreateVendorsFromQuickBooksOnlineAndIdentifyPaymentFrequency(ITestOutputHelper output)
        {
            Output = output;
        }

        //[Fact]
        public void Run()
        {
            var qboClient = Factory .CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var awsDbClient = Factory.CreateAmazonDynamoDbClient();

            var activeCustomers = qboClient.QueryAll<Customer>("select * from Customer Where Active = true");
            foreach (var customer in activeCustomers)
            {
                var marchStart = new DateTime(2020, 3, 1).ToString("yyyy-MM-dd");
                var marchEnd = new DateTime(2020, 3, 31).ToString("yyyy-MM-dd");
                var oneReceiptMarch = HasOneReceipt(qboClient, customer.Id,
                    marchStart, marchEnd);
                var oneReceiptMay = HasOneReceipt(qboClient, customer.Id,
                    new DateTime(2020, 5, 1).ToString("yyyy-MM-dd"),
                    new DateTime(2020, 5, 31).ToString("yyyy-MM-dd"));
                var juneStart = new DateTime(2020, 6, 1).ToString("yyyy-MM-dd");
                var juneEnd = new DateTime(2020, 6, 30).ToString("yyyy-MM-dd");
                var oneReceiptJune = HasOneReceipt(qboClient, customer.Id,
                    juneStart,
                    juneEnd);

                string paymentFrequency = oneReceiptMarch && oneReceiptMay && oneReceiptJune ? "monthly" : string.Empty;

                decimal? rentPrice = null;

                var salesReceipts = qboClient.QueryAll<SalesReceipt>(
                    $"select * from SalesReceipt Where TxnDate >= '{marchStart}' and TxnDate <= '{marchEnd}' and CustomerRef = '{customer.Id}'");
                var invoices = qboClient.QueryAll<Invoice>(
                    $"select * from Invoice Where TxnDate >= '{marchStart}' and TxnDate <= '{marchEnd}' and CustomerRef = '{customer.Id}'");

                if (paymentFrequency == "monthly")
                {
                    if (salesReceipts.SingleOrDefault() != null)
                    {
                        rentPrice = salesReceipts.Single().TotalAmount;
                    }
                    if (invoices.SingleOrDefault() != null)
                    {
                        rentPrice = invoices.Single().TotalAmount;
                    }
                }

                new VendorService().Create(awsDbClient, int.Parse(customer.Id), true, paymentFrequency, rentPrice);
            }
        }

        public bool HasOneReceipt(QuickBooksOnlineClient qboClient, string customerId, string startDate, string endDate)
        {
            var salesReceipts = qboClient.QueryAll<SalesReceipt>(
                $"select * from SalesReceipt Where TxnDate >= '{startDate}' and TxnDate <= '{endDate}' and CustomerRef = '{customerId}'");
            var invoices = qboClient.QueryAll<Invoice>(
                $"select * from Invoice Where TxnDate >= '{startDate}' and TxnDate <= '{endDate}' and CustomerRef = '{customerId}'");

            return (salesReceipts.Count + invoices.Count) == 1;
        }

    }
}
