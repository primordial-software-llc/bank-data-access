using System;
using PropertyRentalManagement.Reports;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode.Reports
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
            var start = new DateTime(2020, 6, 29);
            var end = new DateTime(2020, 7, 5);
            IncomeReport.PrintReport(
                start,
                end, new XUnitLogger(Output),
                Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output)));
        }
    }
}
