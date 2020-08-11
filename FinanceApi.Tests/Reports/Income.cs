using System;
using FinanceApi.Tests.InfrastructureAsCode;
using PropertyRentalManagement.Reports;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.Reports
{
    public class Income
    {
        private ITestOutputHelper Output { get; }

        public Income(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void PrintDailyIncome()
        {
            var start = new DateTime(2020, 8, 9);
            var end = new DateTime(2020, 8, 9);
            IncomeReport.PrintReport(
                start,
                end, new XUnitLogger(Output),
                Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output)));
        }
    }
}
