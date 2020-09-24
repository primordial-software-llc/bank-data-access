using System;
using System.Linq;
using FinanceApi.Tests.InfrastructureAsCode;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.Reports;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.Reports
{
    public class Income
    {
        private ITestOutputHelper Output { get; }

        public Income(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void PrintRentalIncomeForMonth()
        {
            var start = new DateTime(2020, 8, 1);
            var end = new DateTime(2020, 8, 31);
            var client = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));

            var invoiceQuery = $"select * from Invoice Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            var invoices = client.QueryAll<Invoice>(invoiceQuery)
                .Where(x => !PropertyRentalManagement.Constants.NonRentalCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
                .ToList();


            var invoiceLines = invoices.SelectMany(x => x.Line).ToList();

            var depositInvoiceLines = invoiceLines.Where(x =>
                x.SalesItemLineDetail != null &&
                x.SalesItemLineDetail.ItemRef.Value == "212").ToList();
            var invoiceDepositTotal = depositInvoiceLines.Sum(x => x.Amount.GetValueOrDefault());


            string salesReceiptQuery = $"select * from SalesReceipt Where TxnDate >= '{start:yyyy-MM-dd}' and TxnDate <= '{end:yyyy-MM-dd}'";
            var salesReceipts = client.QueryAll<SalesReceipt>(salesReceiptQuery)
                .Where(x => !PropertyRentalManagement.Constants.NonRentalCustomerIds.Contains(int.Parse(x.CustomerRef.Value)))
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

        [Fact]
        public void PrintDailyCash()
        {
            var start = new DateTime(2020, 9, 19);
            var end = new DateTime(2020, 9, 20);
            IncomeReport.PrintReport(
                start,
                end,
                new XUnitLogger(Output),
                Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output)));
        }
    }
}
