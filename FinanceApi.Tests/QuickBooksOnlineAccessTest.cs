using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FinanceApi.RequestModels;
using FinanceApi.Routes.Authenticated.PointOfSale;
using FinanceApi.Taxes;
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

        //[Fact]
        public void SendInvoicesToCustomersQuickBooksInstance()
        {
            var qboClient = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));

            var invoiceFiles = Directory
                .GetFiles(@"C:\Users\peon\Desktop\private\primordial-llc\customers\lakeland-mi-pueblo\invoices", "*.xlsx")
                .ToList();
            var total = 0m;
            foreach (var invoiceFile in invoiceFiles)
            {
                var invoiceDate = DateTime.Parse(GetInvoiceDate(invoiceFile)).ToString("yyyy-MM-dd");

                var invoiceItems = GetInvoiceLineItemsForLakelandMiPueblo(invoiceFile)
                    .Where(x => !string.IsNullOrWhiteSpace(x.Amount) && x.Amount != "$-")
                    .ToList();

                var billableTimeEntries = invoiceItems.Where(x =>
                    string.Equals(x.Description, "Hourly business consulting for customer sales processes and bookkeeping", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Description, "Hourly consulting, bookkeeping and software development", StringComparison.OrdinalIgnoreCase)
                );

                var billableExpenses = invoiceItems.Where(x =>
                    !string.Equals(x.Description, "Hourly business consulting for customer sales processes and bookkeeping", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(x.Description, "Hourly consulting, bookkeeping and software development", StringComparison.OrdinalIgnoreCase)
                );

                var purchase = new Purchase
                {
                    TxnDate = invoiceDate,
                    EntityRef = new Reference {Value = 1920.ToString(), Type = "Vendor"},
                    Line = new List<PurchaseLine>(),
                    PaymentType = "Cash",
                    AccountRef = new Reference {Value = "61"}
                };

                foreach (var billableTime in billableTimeEntries)
                {
                    purchase.Line.Add(new PurchaseLine
                        {
                            Amount = decimal.Parse(billableTime.Amount.Replace("$", string.Empty)),
                            Description = "Hourly consulting, bookkeeping and software development",
                            DetailType = "AccountBasedExpenseLineDetail",
                            AccountBasedExpenseLineDetail = new AccountBasedExpenseLineDetail
                            {
                                AccountRef = new Reference { Value = "47" }
                            }
                        }
                    );
                }

                foreach (var billableExpense in billableExpenses)
                {
                    purchase.Line.Add(new PurchaseLine
                        {
                            Amount = decimal.Parse(billableExpense.Amount.Replace("$", string.Empty)),
                            Description = billableExpense.Description,
                            DetailType = "AccountBasedExpenseLineDetail",
                            AccountBasedExpenseLineDetail = new AccountBasedExpenseLineDetail
                            {
                                AccountRef = new Reference { Value = "49" }
                            }
                        }
                    );
                }

                Purchase result = qboClient.Create(purchase);
            }
        }


        private string GetInvoiceDate(string file)
        {
            var book = new LinqToExcel.ExcelQueryFactory(file);

            var query =
                from row in book.Worksheet("Invoice")
                let item = new
                {
                    Date = row[4].Cast<string>(),
                }
                select item;

            var items = query.ToList();

            return items[3].Date;
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

    }
}
