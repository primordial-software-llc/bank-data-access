using System;
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
            return transactions.OrderBy(x => x.TransactionDetail.Date).ToList();
        }

        private List<InstitutionAccountTransaction> GetTransactionsForBank(
            BankAccessClient client,
            string accessToken,
            string startDate,
            string endDate)
        {
            var transactionResponses = new List<InstitutionAccountTransaction>();
            int count = 100;
            int offset = 0;
            int currentPage = 0;
            int pages;
            JObject institution = null;
            do
            {
                var transactions = client.GetTransactions(accessToken, startDate, endDate, count, offset);
                double exactPages = transactions.TotalTransactions / (double) count;
                pages = (int) Math.Ceiling(Convert.ToDouble(exactPages));
                if (institution == null)
                {
                    institution = client.GetInstitution(transactions.Item.InstitutionId);
                }
                foreach (var transaction in transactions.Transactions)
                {
                    var transactionResponse = new InstitutionAccountTransaction
                    {
                        InstitutionName = institution["institution"]["name"].Value<string>(),
                        TransactionDetail = transaction,
                        Account = transactions
                            .Accounts
                            .First(x => x.AccountId == transaction.AccountId)
                    };
                    transactionResponses.Add(transactionResponse);
                }
                offset += count;
                currentPage += 1;
            } while (currentPage < pages);
            return transactionResponses;
        }
    }
}
