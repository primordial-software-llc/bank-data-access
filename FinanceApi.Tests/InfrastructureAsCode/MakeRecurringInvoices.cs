using System;
using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode
{
    public class MakeRecurringInvoices
    {
        private ITestOutputHelper Output { get; }

        public MakeRecurringInvoices(ITestOutputHelper output)
        {
            Output = output;
        }

        //[Fact]
        public void MakeMonthlyInvoices()
        {
            var monthStart = new DateTime(2020, 7, 1);
            var monthEnd = new DateTime(monthStart.Year, monthStart.Month,
                DateTime.DaysInMonth(monthStart.Year, monthStart.Month));

            var vendorClient = new VendorService();
            var monthlyVendors = vendorClient.GetByPaymentFrequency(
                Factory.CreateAmazonDynamoDbClient(),
                "monthly");

            var qboClient = Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output));
            var saleReportService = new SaleReportService();
            foreach (var monthlyVendor in monthlyVendors)
            {
                var customer = qboClient
                    .QueryAll<Customer>($"select * from customer where id = '{monthlyVendor.QuickBooksOnlineId}'")
                    .SingleOrDefault();
                if (customer == null) // Merged or inactivated.
                {
                    continue;
                }
                var salesToVendor = saleReportService.GetSales(
                    qboClient,
                    monthStart,
                    monthEnd,
                    monthlyVendor.QuickBooksOnlineId.ToString(),
                    new List<int>());
                if (salesToVendor.Invoices.Any() || salesToVendor.SalesReceipts.Any())
                {
                    continue;
                }

                decimal quantity = 1;
                decimal taxableAmount = monthlyVendor.RentPrice.GetValueOrDefault() / 1.068m;
                var invoice = new Invoice
                {
                    TxnDate = monthStart.ToString("yyyy-MM-dd"),
                    CustomerRef = new Reference { Value = customer.Id.ToString() },
                    Line = new List<InvoiceLine>
                    {
                        new InvoiceLine
                        {
                            DetailType = "SalesItemLineDetail",
                            SalesItemLineDetail = new SalesItemLineDetail
                            {
                                ItemRef = new Reference { Value = PropertyRentalManagement.Constants.QUICKBOOKS_PRODUCT_RENT.ToString() },
                                Quantity = quantity,
                                TaxCodeRef = new Reference { Value = PropertyRentalManagement.Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE },
                                UnitPrice = taxableAmount
                            },
                            Amount = quantity * taxableAmount
                        }
                    },
                    TxnTaxDetail = new TxnTaxDetail
                    {
                        TxnTaxCodeRef = new Reference { Value = PropertyRentalManagement.Constants.QUICKBOOKS_RENTAL_TAX_RATE.ToString() }
                    },
                    PrivateNote = monthlyVendor.Memo,
                    SalesTermRef = new Reference { Value = PropertyRentalManagement.Constants.QUICKBOOKS_TERMS_DUE_NOW.ToString() }
                };
                Output.WriteLine(customer.DisplayName);
                invoice = qboClient.Create("invoice", invoice);
            }
        }

    }
}
