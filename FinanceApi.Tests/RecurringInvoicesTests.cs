using System;
using PropertyRentalManagement.BusinessLogic;
using Xunit;

namespace FinanceApi.Tests
{
    public class RecurringInvoicesTests
    {
        [Fact]
        public void TestStartOfWeek()
        {
            Assert.Equal("2020-07-13", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 13), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-13", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 14), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-13", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 15), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-13", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 16), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-13", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 17), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-13", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 18), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-13", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 19), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-20", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 20), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-20", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 21), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-20", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 22), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-20", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 23), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-20", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 24), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-20", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 25), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-20", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 26), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-27", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 27), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-27", RecurringInvoices.StartOfWeek(new DateTime(2020, 7, 28), DayOfWeek.Monday).ToString("yyyy-MM-dd"));
        }

        [Fact]
        public void TestEndOfWeek()
        {
            Assert.Equal("2020-07-19", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 13), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-19", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 14), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-19", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 15), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-19", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 16), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-19", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 17), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-19", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 18), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-19", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 19), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-26", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 20), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-26", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 21), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-26", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 22), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-26", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 23), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-26", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 24), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-26", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 25), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-07-26", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 26), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-08-02", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 27), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-08-02", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 28), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-08-02", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 29), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-08-02", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 30), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-08-02", RecurringInvoices.EndOfWeek(new DateTime(2020, 7, 31), DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
        }

        [Fact]
        public void StartOfMonth()
        {
            Assert.Equal("2020-07-01", RecurringInvoices.StartOfMonth(new DateTime(2020, 7, 13)).ToString("yyyy-MM-dd"));
            Assert.Equal("2020-08-01", RecurringInvoices.StartOfMonth(new DateTime(2020, 8, 14)).ToString("yyyy-MM-dd"));
        }
    }
}
