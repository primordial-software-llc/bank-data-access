using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceApi.DatabaseModel;
using FinanceApi.ResponseModels;
using Newtonsoft.Json.Linq;

namespace FinanceApi.BusinessLogic
{
    public class TransactionAggregator
    {

        public List<InstitutionAccountTransaction> GetTransactions(
            BankAccessClient client,
            FinanceUser user,
            string startDate,
            string endDate)
        {
            var transactions = new List<InstitutionAccountTransaction>();
            Parallel.ForEach(
                user.BankLinks ?? new List<BankLink>(),
                new ParallelOptions {MaxDegreeOfParallelism = 10},
                bankLink =>
                {
                    transactions.AddRange(GetTransactionsForBank(
                        client,
                        bankLink.AccessToken,
                        startDate,
                        endDate));
                });
            return transactions;
        }

        private List<InstitutionAccountTransaction> GetTransactionsForBank(
            BankAccessClient client,
            string accessToken,
            string startDate,
            string endDate)
        {
            var transactions = client.GetTransactions(accessToken, startDate, endDate);
            var institution = client.GetInstitution(transactions.Item.InstitutionId);
            var transactionResponses = new List<InstitutionAccountTransaction>();
            foreach (var transaction in transactions.Transactions)
            {
                var transactionResponse = new InstitutionAccountTransaction
                {
                    InstitutionName = institution["institution"]["name"].Value<string>(),
                    TransactionDetail = transaction,
                    Account = transactions
                        .Accounts
                        .First(x => x.AccountId == transaction["account_id"].Value<string>())
                };
                transactionResponses.Add(transactionResponse);
            }
            return transactionResponses;
        }
    }
}
