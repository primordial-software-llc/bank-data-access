using System.Linq;
using FinanceApi.BusinessLogic;
using FinanceApi.DatabaseModel;
using FinanceApi.Tests.InfrastructureAsCode;
using PropertyRentalManagement.DataServices;
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

        [Fact]
        public void Create()
        {

            var client = Factory.CreateAmazonDynamoDbClientForBanking();
            var userClient = new DatabaseClient<FinanceUser>(client);
            var user = userClient.Get(new FinanceUser { Email = "timg456789@yahoo.com" });

            var transactions = new TransactionAggregator()
                .GetTransactions(
                    bankClient,
                    user,
                    "2020-06-01",
                    "2020-08-31")
                .ToList();

            var recurringTransactions = new RecurringTransactionReport().GetRecurringTransactions(transactions);

            Output.WriteLine(recurringTransactions.Count.ToString());
            foreach (var transaction in recurringTransactions)
            {
                Output.WriteLine($"{transaction.InstitutionName} - {transaction.TransactionDetail.Name} - {transaction.TransactionDetail.Amount}");
            }
        }
    }
}