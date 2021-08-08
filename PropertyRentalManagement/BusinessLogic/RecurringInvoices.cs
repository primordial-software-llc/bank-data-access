using System;
using System.Collections.Generic;
using System.Linq;
using AwsDataAccess;
using Newtonsoft.Json;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;

namespace PropertyRentalManagement.BusinessLogic
{
    public class RecurringInvoices
    {
        public enum Frequency
        {
            Weekly = 0,
            Monthly
        }

        private VendorService VendorService { get; }
        private QuickBooksOnlineClient QuickBooksClient { get; }
        private decimal TaxRate { get; }
        private ILogging Logging { get; }

        public RecurringInvoices(VendorService vendorService, QuickBooksOnlineClient quickBooksClient, decimal taxRate, ILogging logging)
        {
            VendorService = vendorService;
            QuickBooksClient = quickBooksClient;
            TaxRate = taxRate;
            Logging = logging;
        }

        public List<Invoice> CreateInvoicesForFrequency(DateTime date, Frequency frequency)
        {
            var dateRange = GetRange(frequency, date);
            Logging.Log($"Creating {frequency} on {date:yyyy-MM-dd} for {dateRange.Start:yyyy-MM-dd} to {dateRange.End:yyyy-MM-dd}.");
            var connectionLock = QuickBooksClient.GetConnectionForLocks();
            if (frequency == Frequency.Weekly && connectionLock.WeeklyInvoiceLock ||
                frequency == Frequency.Monthly && connectionLock.MonthlyInvoiceLock)
            {
                throw new Exception($"Can't create {frequency} invoices. {frequency} invoice lock is enabled. Invoices are being created by another process.");
            }
            QuickBooksClient.LockInvoices(frequency, true);
            try
            {
                var invoices = CreateInvoices(dateRange, frequency);
                Logging.Log($"Created {invoices.Count} {frequency} invoices for {date:O}.");
                foreach (var invoice in invoices)
                {
                    Logging.Log($"Created {frequency} invoice for {invoice.CustomerRef.Name} - {invoice.CustomerRef.Value}");
                }
                Logging.Log("Newly created invoices: " + JsonConvert.SerializeObject(invoices));
                return invoices;
            }
            finally
            {
                QuickBooksClient.LockInvoices(frequency, false);
            }
        }

        private List<Invoice> CreateInvoices(DateRange dateRange, Frequency frequency)
        {
            var invoiceQuery = $"select * from Invoice Where TxnDate >= '{dateRange.Start:yyyy-MM-dd}' and TxnDate <= '{dateRange.End:yyyy-MM-dd}'";
            var allInvoices = QuickBooksClient.QueryAll<Invoice>(invoiceQuery);
            var allActiveCustomers = QuickBooksClient.QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);
            var vendors = new ActiveVendorSearch().GetActiveVendors(allActiveCustomers, VendorService, frequency);
            var newInvoices = new List<Invoice>();
            foreach (var vendor in vendors.Values)
            {
                var vendorInvoices = allInvoices.Where(x => x.CustomerRef.Value == vendor.QuickBooksOnlineId.ToString());
                if (!vendorInvoices.Any())
                {
                    var invoiceDate = frequency == Frequency.Weekly ? dateRange.End : dateRange.Start;
                    newInvoices.Add(CreateInvoice(invoiceDate, allActiveCustomers[vendor.QuickBooksOnlineId], vendor));
                }
            }
            var paymentApplicator = new PaymentApplicator(QuickBooksClient);
            foreach (var invoice in newInvoices)
            {
                paymentApplicator.ApplyUnappliedPaymentsToInvoice(invoice);
            }
            return newInvoices;
        }

        private Invoice CreateInvoice(DateTime date, Customer customer, DatabaseModel.Vendor vendor)
        {
            decimal quantity = 1;
            decimal taxableAmount = vendor.RentPrice.GetValueOrDefault() / (1 + TaxRate);
            var invoice = new Invoice
            {
                TxnDate = date.ToString("yyyy-MM-dd"),
                CustomerRef = new Reference { Value = customer.Id.ToString() },
                Line = new List<SalesLine>
                {
                    new SalesLine
                    {
                        DetailType = "SalesItemLineDetail",
                        SalesItemLineDetail = new SalesItemLineDetail
                        {
                            ItemRef = new Reference { Value = Constants.QUICKBOOKS_PRODUCT_RENT.ToString() },
                            Quantity = quantity,
                            TaxCodeRef = new Reference { Value = Accounting.Constants.QUICKBOOKS_INVOICE_LINE_TAXABLE },
                            UnitPrice = taxableAmount
                        },
                        Amount = quantity * taxableAmount
                    }
                },
                TxnTaxDetail = new TxnTaxDetail
                {
                    TxnTaxCodeRef = new Reference { Value = Constants.QUICKBOOKS_TAX_RATE_POLK_COUNTY_RENTAL.ToString() }
                },
                PrivateNote = vendor.Memo,
                SalesTermRef = new Reference { Value = Constants.QUICKBOOKS_TERMS_DUE_NOW.ToString() }
            };
            return QuickBooksClient.Create(invoice);
        }

        public static DateRange GetRange(Frequency frequency, DateTime date)
        {
            if (frequency == Frequency.Weekly)
            {
                return GetWeekDateRange(date);
            }
            if (frequency == Frequency.Monthly)
            {
                return GetMonthDateRange(date);
            }
            throw new Exception($"Can't lock invoices due to unknown frequency of {frequency}");
        }

        public static DateRange GetWeekDateRange(DateTime date)
        {
            var start = StartOfWeek(date, DayOfWeek.Monday);
            var end = EndOfWeek(date, DayOfWeek.Sunday);
            return new DateRange {Start = start, End = end};
        }

        public static DateRange GetMonthDateRange(DateTime date)
        {
            var start = StartOfMonth(date);
            var end = new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month));
            return new DateRange {Start = start, End = end};
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

    }
}
