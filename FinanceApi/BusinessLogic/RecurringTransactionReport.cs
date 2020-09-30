using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FinanceApi.ResponseModels;

namespace FinanceApi.BusinessLogic
{
    public class RecurringTransactionReport
    {
        public List<InstitutionAccountTransaction> GetRecurringTransactions(
            List<InstitutionAccountTransaction> transactions
        )
        {
            var transactionsByDate = transactions.GroupBy(x =>
                    GetDate(x.TransactionDetail.Date).Month)
                .ToList();

            var recurringTransactions = new List<InstitutionAccountTransaction>();
            foreach (var transaction in transactions)
            {
                var occursForEachMonth = transactionsByDate
                    .All(x =>
                        x.Any(y => IsEquivalent(transaction, y))
                    );

                if (!occursForEachMonth)
                {
                    continue;
                }

                if (!recurringTransactions
                    .Any(x => IsEquivalent(transaction, x)))
                {
                    recurringTransactions.Add(transaction);
                }
            }

            return recurringTransactions;
        }

        private bool IsEquivalent(InstitutionAccountTransaction a, InstitutionAccountTransaction b)
        {
            return string.Equals(
                       CleanTransactionName(a.TransactionDetail.Name),
                       CleanTransactionName(b.TransactionDetail.Name),
                       StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(a.InstitutionName, b.InstitutionName, StringComparison.OrdinalIgnoreCase)
                ;
        }

        public static string CleanTransactionName(string transactionName)
        {
            return Regex.Replace(transactionName, @"[\d-]", string.Empty);
        }

        private DateTime GetDate(string date)
        {
            return DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
