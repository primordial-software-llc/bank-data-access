using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DataServices;
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

        [Fact]
        public void CreateWeeklyInvoices()
        {
            var vendorService = new VendorService(Factory.CreateAmazonDynamoDbClient());
            var weeklyInvoices = new RecurringInvoices().CreateInvoices(
                new DateTime(2020, 7, 12),
                new DateTime(2020, 7, 12),
                vendorService,
                Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output)),
                "weekly");
            Output.WriteLine($"Created {weeklyInvoices.Count} weekly invoices.");
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
    }
}
