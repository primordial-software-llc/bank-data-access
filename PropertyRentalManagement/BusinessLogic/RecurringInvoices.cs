using System;
using System.Collections.Generic;
using System.Linq;
using AwsDataAccess;
using Newtonsoft.Json;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;

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
        private ILogging Logging { get; }

        public RecurringInvoices(VendorService vendorService, QuickBooksOnlineClient quickBooksClient, ILogging logging)
        {
            VendorService = vendorService;
            QuickBooksClient = quickBooksClient;
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
            var locations = VendorService.GetLocations();
            var newInvoices = new List<Invoice>();
            foreach (var vendor in vendors.Values)
            {
                var vendorInvoices = allInvoices.Where(x => x.CustomerRef.Value == vendor.QuickBooksOnlineId.ToString());
                if (!vendorInvoices.Any())
                {
                    var invoiceDate = frequency == Frequency.Weekly ? dateRange.End : dateRange.Start;
                    var vendorLocation = VendorService
                        .GetVendorLocations(locations, vendor.Id)
                        .First();
                    var invoiceLocation = locations.First(x => x.Id == vendorLocation.LocationId);
                    var invoiceModel = new InvoiceCreation().CreateInvoiceModel(
                        QuickBooksClient,
                        allActiveCustomers[vendor.QuickBooksOnlineId].Id.ToString(),
                        vendor.RentPrice.GetValueOrDefault(),
                        invoiceDate.ToString("yyyy-MM-dd"),
                        vendor.Memo,
                        invoiceLocation.RentProductId.ToString(),
                        invoiceLocation.SalesRentalTaxRateId.ToString(),
                        true);
                    invoiceModel = QuickBooksClient.Create(invoiceModel);
                    newInvoices.Add(invoiceModel);
                }
            }
            var paymentApplicator = new PaymentApplicator(QuickBooksClient);
            foreach (var invoice in newInvoices)
            {
                paymentApplicator.ApplyUnappliedPaymentsToInvoice(invoice);
            }
            return newInvoices;
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
