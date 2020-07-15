using System;
using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;

namespace PropertyRentalManagement.BusinessLogic
{
    public class RecurringInvoices
    {
        private const string FREQUENCY_WEEKLY = "weekly";
        private const string FREQUENCY_MONTHLY = "monthly";

        public List<Invoice> CreateWeeklyInvoices(DateTime date, VendorService vendorService,
            QuickBooksOnlineClient quickBooksClient)
        {
            var start = StartOfWeek(date, DayOfWeek.Monday);
            var end = EndOfWeek(date, DayOfWeek.Sunday);
            return CreateInvoices(start, end, vendorService, quickBooksClient, FREQUENCY_WEEKLY);
        }

        public List<Invoice> CreateMonthlyInvoices(DateTime date, VendorService vendorService, QuickBooksOnlineClient quickBooksClient)
        {
            var monthStart = StartOfMonth(date);
            var monthEnd = new DateTime(monthStart.Year, monthStart.Month,
                DateTime.DaysInMonth(monthStart.Year, monthStart.Month));
            return new RecurringInvoices().CreateInvoices(
                monthStart,
                monthEnd,
                vendorService,
                quickBooksClient,
                FREQUENCY_MONTHLY);
        }

        public List<Invoice> CreateInvoices(DateTime start, DateTime end, VendorService vendorService, QuickBooksOnlineClient quickBooksClient, string frequency)
        {
            var allInvoices = new SalesReportService().GetInvoices(quickBooksClient, start, end);
            var allActiveCustomers = quickBooksClient.QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);
            var vendors = new ActiveVendorSearch().GetActiveVendors(allActiveCustomers, vendorService, frequency);
            var newInvoices = new List<Invoice>();
            foreach (var vendor in vendors.Values)
            {
                var vendorInvoices = allInvoices.Where(x => x.CustomerRef.Value == vendor.QuickBooksOnlineId.ToString());
                if (!vendorInvoices.Any())
                {
                    var invoiceDate = string.Equals(FREQUENCY_WEEKLY, frequency, StringComparison.OrdinalIgnoreCase) ? end : start;
                    newInvoices.Add(CreateInvoice(quickBooksClient, invoiceDate, allActiveCustomers[vendor.QuickBooksOnlineId], vendor));
                }
            }
            var paymentApplicator = new PaymentApplicator(quickBooksClient);
            foreach (var invoice in newInvoices)
            {
                paymentApplicator.ApplyUnappliedPaymentsToInvoice(invoice);
            }
            return newInvoices;
        }

        private Invoice CreateInvoice(QuickBooksOnlineClient qboClient, DateTime date, Customer customer, DatabaseModel.Vendor vendor)
        {
            decimal quantity = 1;
            decimal taxableAmount = vendor.RentPrice.GetValueOrDefault() / 1.068m;
            var invoice = new Invoice
            {
                TxnDate = date.ToString("yyyy-MM-dd"),
                CustomerRef = new Reference { Value = customer.Id.ToString() },
                Line = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        DetailType = "SalesItemLineDetail",
                        SalesItemLineDetail = new SalesItemLineDetail
                        {
                            ItemRef = new Reference { Value = Constants.QUICKBOOKS_PRODUCT_RENT.ToString() },
                            Quantity = quantity,
                            TaxCodeRef = new Reference { Value = Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE },
                            UnitPrice = taxableAmount
                        },
                        Amount = quantity * taxableAmount
                    }
                },
                TxnTaxDetail = new TxnTaxDetail
                {
                    TxnTaxCodeRef = new Reference { Value = Constants.QUICKBOOKS_RENTAL_TAX_RATE.ToString() }
                },
                PrivateNote = vendor.Memo,
                SalesTermRef = new Reference { Value = Constants.QUICKBOOKS_TERMS_DUE_NOW.ToString() }
            };
            return qboClient.Create(invoice);
        }

        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(DateTime date, DayOfWeek endOfWeek)
        {
            int diff = (endOfWeek - date.DayOfWeek + 7) % 7;
            return date.AddDays(diff).Date;
        }

        public static DateTime StartOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime EndOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }
    }
}
