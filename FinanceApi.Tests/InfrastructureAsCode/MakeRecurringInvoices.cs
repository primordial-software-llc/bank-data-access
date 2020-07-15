using System;
using Newtonsoft.Json;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DataServices;
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

        [Fact]
        public void CreateWeeklyInvoices()
        {
            var vendorService = new VendorService(Factory.CreateAmazonDynamoDbClient());

            var start = StartOfWeek(DateTime.Now.Date, DayOfWeek.Monday);
            var end = EndOfWeek(DateTime.Now.Date, DayOfWeek.Sunday);

            var weeklyInvoices = new RecurringInvoices().CreateInvoices(
                start,
                end,
                vendorService,
                Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output)),
                "weekly");
            Output.WriteLine($"Created {weeklyInvoices.Count} weekly invoices for {start:yyyy-MM-dd} to {end:yyyy-MM-dd}.");
            foreach (var weeklyInvoice in weeklyInvoices)
            {
                Output.WriteLine($"Created weekly invoice for {weeklyInvoice.CustomerRef.Name} - {weeklyInvoice.CustomerRef.Value}");
            }
            Output.WriteLine(JsonConvert.SerializeObject(weeklyInvoices));
        }

        //[Fact]
        public void MakeMonthlyInvoices()
        {
            var monthStart = new DateTime(2020, 7, 1);
            var monthEnd = new DateTime(monthStart.Year, monthStart.Month,
                DateTime.DaysInMonth(monthStart.Year, monthStart.Month));
            var vendorService = new VendorService(Factory.CreateAmazonDynamoDbClient());
            var weeklyInvoices = new RecurringInvoices().CreateInvoices(
                monthStart,
                monthEnd,
                vendorService,
                Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output)),
                "monthly");
            Output.WriteLine($"Created {weeklyInvoices.Count} monthly invoices.");
            foreach (var weeklyInvoice in weeklyInvoices)
            {
                Output.WriteLine($"Created monthly invoice for {weeklyInvoice.CustomerRef.Name} - {weeklyInvoice.CustomerRef.Value}");
            }
            Output.WriteLine(JsonConvert.SerializeObject(weeklyInvoices));
        }

        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        public static DateTime EndOfWeek(DateTime dt, DayOfWeek endOfWeek)
        {
            int diff = (endOfWeek - dt.DayOfWeek + 7) % 7;
            return dt.AddDays(diff).Date;
        }
    }
}
