using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using FinanceApi.Tests.InfrastructureAsCode;
using PropertyRentalManagement;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.Reports
{
    public class IncomeReport
    {
        private ITestOutputHelper Output { get; }

        public IncomeReport(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void PrintReceiptsCashBasisIncome()
        {
            var client = new AwsDataAccess.DatabaseClient<ReceiptSaveResult>(
                Factory.CreateAmazonDynamoDbClient(),
                new ConsoleLogger());

            var start = "2020-10-17T14:40:52Z";
            var end = "2020-10-19T14:40:52Z";

            var scanRequest = new ScanRequest // Can't use between on an id. It must be used on a range which requires an id. Until requests take longer than 30 seconds and timeout this solution is adequate.
            {
                TableName = new ReceiptSaveResult().GetTable(),
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":start", new AttributeValue {S = start }},
                    {":end", new AttributeValue {S = end }}
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#timestamp", "timestamp" }
                },
                FilterExpression = "#timestamp between :start and :end"
            };
            var results = client.ScanAll(scanRequest);
        }

        [Fact]
        public void PrintAccrualRentalIncomeForMonth()
        {
            var start = new DateTime(2020, 8, 1);
            var end = new DateTime(2020, 8, 31);
            var client = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));

            var invoiceQuery = $"select * from Invoice Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            var invoices = client.QueryAll<Invoice>(invoiceQuery)
                .Where(x => !Constants.NonRentalCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();


            var invoiceLines = invoices.SelectMany(x => x.Line).ToList();

            var depositInvoiceLines = invoiceLines.Where(x =>
                x.SalesItemLineDetail != null &&
                x.SalesItemLineDetail.ItemRef.Value == "212").ToList();
            var invoiceDepositTotal = depositInvoiceLines.Sum(x => x.Amount.GetValueOrDefault());


            string salesReceiptQuery = $"select * from SalesReceipt Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            var salesReceipts = client.QueryAll<SalesReceipt>(salesReceiptQuery)
                .Where(x => !Constants.NonRentalCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();

            var salesReceiptLines = salesReceipts.SelectMany(x => x.Line).ToList();
            var depositSalesReceiptLines = salesReceiptLines.Where(x =>
                x.SalesItemLineDetail != null &&
                x.SalesItemLineDetail.ItemRef.Value == "212").ToList();
            var salesReceiptDepositTotal = depositSalesReceiptLines.Sum(x => x.Amount.GetValueOrDefault());

            var total = invoices.Sum(x => x.TotalAmount.GetValueOrDefault() - x.Balance.GetValueOrDefault());
            total += salesReceipts.Sum(x => x.TotalAmount.GetValueOrDefault());
            total -= invoiceDepositTotal;
            total -= salesReceiptDepositTotal;

            Output.WriteLine(total.ToString());
            Assert.Equal(65633.45m, total);
        }

    }
}
