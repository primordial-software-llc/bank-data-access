using FinanceApi.BusinessLogic;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
{
    public class BankingTests
    {
        private ITestOutputHelper Output { get; }

        public BankingTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void TestCleaningTransactionName()
        {
            Assert.Equal("abcdeFGH", RecurringTransactionReport.CleanTransactionName("abcdeFGH1234567890"));
        }

    }
}