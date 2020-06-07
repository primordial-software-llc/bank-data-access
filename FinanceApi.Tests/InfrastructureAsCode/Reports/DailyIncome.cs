using System;
using PropertyRentalManagement.Reports;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode.Reports
{
    public class DailyIncome
    {
        private ITestOutputHelper Output { get; }

        public DailyIncome(ITestOutputHelper output)
        {
            Output = output;
        }

        /// <summary>
        /// Some income doesn't get entered into QB as it happens.
        /// These sales are in excel.
        /// </summary>
        [Fact]
        public void PrintDailyIncome()
        {
            var reportDate = new DateTime(2020, 3, 15).ToString("yyyy-MM-dd");
            DailyIncomeReport.PrintReport(reportDate, new XUnitLogger(Output),
                Factory.CreateQuickBooksOnlineClient(new XUnitLogger(Output)));
        }

    }
}
