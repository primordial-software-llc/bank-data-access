using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApi.Tests.InfrastructureAsCode;
using PropertyRentalManagement.Clover;
using PropertyRentalManagement.Clover.Models;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using Xunit;
using Xunit.Abstractions;
using Reference = PropertyRentalManagement.QuickBooksOnline.Models.Reference;

namespace FinanceApi.Tests
{
    public class CreateFromClover
    {
        private ITestOutputHelper Output { get; }

        const int ACCOUNT_RESTAURANT = 1864;
        const int PRODUCT_RESTAURANT = 238;


        public CreateFromClover(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void SendCloverIncomeToQuickBooksOnlineForMonth()
        {
            var date = new DateTime(2020, 9, 1);
            var originalMonth = date.Month;

            var qboClient = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var client = new CloverClient(
                Environment.GetEnvironmentVariable("CLOVER_MI_PUEBLO_CHICKEN_ACCESS_TOKEN"),
                Environment.GetEnvironmentVariable("CLOVER_MI_PUEBLO_CHICKEN_MERCHANT_ID"),
                new XUnitLogger(Output)
            );

            var restaurantCustomer = qboClient
                .QueryAll<Customer>($"select * from Customer where Id = '{ACCOUNT_RESTAURANT}'")
                .First();

            var cashTenderType = client.QueryAll<Tender>($"tenders")
                .Single(x => string.Equals(x.LabelKey, "com.clover.tender.cash", StringComparison.OrdinalIgnoreCase));

            do
            {
                var salesReceipts = qboClient.QueryAll<SalesReceipt>(
                    $"select * from SalesReceipt where CustomerRef = '{ACCOUNT_RESTAURANT}' and TxnDate = '{date:yyyy-MM-dd}'");

                if (salesReceipts.Any())
                {
                    Output.WriteLine($"Skipping {date:yyyy-MM-dd}, because a sales receipt already exists.");
                }
                else
                {
                    var today = new DateTimeOffset(date).ToUnixTimeMilliseconds();
                    var tomorrow = new DateTimeOffset(date.AddDays(1)).ToUnixTimeMilliseconds();
                    var result = client.QueryAll<Payment>($"payments?filter=createdTime>={today}&filter=createdTime<{tomorrow}");

                    var cashPayments = result.Where(x => x.Tender.Id == cashTenderType.Id).ToList();
                    var cardPayments = result.Where(x => x.Tender.Id != cashTenderType.Id).ToList();

                    CreateSalesReceipt(
                        qboClient,
                        restaurantCustomer.Id,
                        restaurantCustomer.DefaultTaxCodeRef.Value,
                        PRODUCT_RESTAURANT,
                        date,
                        GetTotal(cashPayments),
                        "Restaurant sales using cash in Clover Register");

                    CreateSalesReceipt(
                        qboClient,
                        restaurantCustomer.Id,
                        restaurantCustomer.DefaultTaxCodeRef.Value,
                        PRODUCT_RESTAURANT,
                        date,
                        GetTotal(cardPayments),
                        "Restaurant sales using credit card in Clover Register");
                }
                date = date.AddDays(1);
            } while (date.Month == originalMonth);
        }

        private void CreateSalesReceipt(
            QuickBooksOnlineClient client,
            int? customerId,
            string taxCode,
            int product,
            DateTime date,
            decimal amount,
            string memo)
        {
            var salesReceipt = new SalesReceipt
            {
                CustomerRef = new Reference { Value = customerId.ToString() },
                TxnDate = date.ToString("yyyy-MM-dd"),
                TotalAmount = amount,
                PrivateNote = memo,
                Line = new List<SalesLine>
                {
                    new SalesLine
                    {
                        Amount = amount,
                        DetailType = "SalesItemLineDetail",
                        SalesItemLineDetail = new SalesItemLineDetail
                        {
                            ItemRef = new Reference { Value = product.ToString() },
                            Quantity = 1,
                            UnitPrice = amount,
                            TaxCodeRef = new Reference { Value = PropertyRentalManagement.Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE }
                        }
                    }
                },
                TxnTaxDetail = new TxnTaxDetail { TxnTaxCodeRef = new Reference { Value = taxCode } }
            };
            client.Create(salesReceipt);
        }

        private decimal GetTotal(List<Payment> payments)
        {
            return payments.Sum(x => x.Amount - x.TaxAmount - x.TipAmount - x.CashbackAmount) / 100;
        }

    }
}
