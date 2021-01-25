using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FinanceApi.Taxes;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
{
    public class ExcelInvoiceIncomeTaxes
    {
        private ITestOutputHelper Output { get; }

        public ExcelInvoiceIncomeTaxes(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void LakelandMiPuebloBillableTotal()
        {
            var invoiceFiles = Directory
                .GetFiles(@"C:\Users\peon\Desktop\private\primordial-llc\customers\lakeland-mi-pueblo\invoices", "*.xlsx")
                .ToList();
            var total = 0m;
            foreach (var invoiceFile in invoiceFiles)
            {
                var invoiceItems = GetInvoiceLineItemsForLakelandMiPueblo(invoiceFile)
                    .Where(x => !string.IsNullOrWhiteSpace(x.Amount) && x.Amount != "$-")
                    .ToList();

                var billableTime = invoiceItems.First(x =>
                    string.Equals(x.Description, "Hourly business consulting for customer sales processes and bookkeeping", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Description, "Hourly consulting, bookkeeping and software development", StringComparison.OrdinalIgnoreCase)
                );

                var billableExpense = invoiceItems.Where(x =>
                    !string.Equals(x.Description, "Hourly business consulting for customer sales processes and bookkeeping", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(x.Description, "Hourly consulting, bookkeeping and software development", StringComparison.OrdinalIgnoreCase)
                );

                total += decimal.Parse(billableTime.Amount.Replace("$", string.Empty));
            }




            Output.WriteLine(total.ToString());
        }

        [Fact]
        public void IndustriasSupersport()
        {
            var invoiceFiles = Directory
                .GetFiles(@"C:\Users\peon\Desktop\private\primordial-llc\customers\industrias-super-sport\sent-invoices", "*.xlsx")
                .ToList();
            var total = 0m;
            foreach (var invoiceFile in invoiceFiles)
            {
                var invoiceItems = GetInvoiceLineItemsForIndustriaSuperSports(invoiceFile)
                    .Where(x => !string.IsNullOrWhiteSpace(x.Amount))
                    .ToList();

                /*
                var commission = invoiceItems.Where(x =>
                    (x.Description ?? string.Empty).Contains("Comission", StringComparison.OrdinalIgnoreCase));
                */

                total += invoiceItems
                    .Sum(x =>
                        decimal.Parse( (!string.IsNullOrWhiteSpace(x.Amount) ? x.Amount : x.AmountB).Replace("$", string.Empty)));
            }
            Output.WriteLine(total.ToString());
        }

        private List<LakeLandMiPuebloInvoiceLineItem> GetInvoiceLineItemsForLakelandMiPueblo(string file)
        {
            var book = new LinqToExcel.ExcelQueryFactory(file);
            var query =
                from row in book.Worksheet("Invoice")
                let item = new LakeLandMiPuebloInvoiceLineItem
                {
                    Description = row[0].Cast<string>(),
                    QuantityAndSubtotal = row[2].Cast<string>(),
                    UnitPrice = row[2].Cast<string>(),
                    Amount = row[4].Cast<string>()
                }
                select item;
            var items = query.ToList();
            var descriptionStart = items
                .First(x => string.Equals(x.Description, "description", StringComparison.OrdinalIgnoreCase));
            var descriptionIndex = items.IndexOf(descriptionStart);
            var subtotalStart = items
                .First(x => string.Equals(x.QuantityAndSubtotal, "subtotal", StringComparison.OrdinalIgnoreCase));
            var subtotalIndex = items.IndexOf(subtotalStart);
            var invoiceItems = items.GetRange(descriptionIndex + 1, subtotalIndex - descriptionIndex - 1);
            return invoiceItems;
        }

        private List<IndustriaSupersportInvoiceLineItem> GetInvoiceLineItemsForIndustriaSuperSports(string file)
        {
            var book = new LinqToExcel.ExcelQueryFactory(file);
            var query =
                from row in book.Worksheet("Invoice")
                let item = new IndustriaSupersportInvoiceLineItem
                {
                    Description = row[0].Cast<string>(),
                    SubtotalA = row[2].Cast<string>(),
                    SubtotalB = row[3].Cast<string>(),
                    SubtotalC = row[5].Cast<string>(),
                    Amount = row[4].Cast<string>(),
                    AmountB = row[7].Cast<string>()
                }
                select item;
            var items = query.ToList();
            var descriptionStart = items
                .First(x => string.Equals(x.Description, "description", StringComparison.OrdinalIgnoreCase));
            var descriptionIndex = items.IndexOf(descriptionStart);
            var subtotalStart = items
                .First(x =>
                    string.Equals(x.SubtotalA, "subtotal", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.SubtotalB, "subtotal", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.SubtotalC, "subtotal", StringComparison.OrdinalIgnoreCase));
            var subtotalIndex = items.IndexOf(subtotalStart);
            var invoiceItems = items.GetRange(descriptionIndex + 1, subtotalIndex - descriptionIndex - 1);
            return invoiceItems;
        }

        class IndustriaSupersportInvoiceLineItem
        {
            public string Description { get; set; }
            public string SubtotalA { get; set; }
            public string SubtotalB { get; set; }
            public string SubtotalC { get; set; }
            public string Amount { get; set; }
            public string AmountB { get; set; }
        }
    }
}
