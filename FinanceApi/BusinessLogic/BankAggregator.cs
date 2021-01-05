using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceApi.DatabaseModel;
using FinanceApi.PlaidModel;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.DataServices;

namespace FinanceApi.BusinessLogic
{
    class BankAggregator
    {
        public FinanceUserBankAccount GetAndCacheFinanceUserBankAccount(
            FinanceUser user,
            DatabaseClient<FinanceUserBankAccount> userBankAccountClient)
        {
            Console.WriteLine("Getting account data from plaid");
            var failedAccounts = new List<FinanceUserBankAccountFailedAccount>();
            var accounts = new ConcurrentBag<AccountBalance>();
            var institutionsDictionary = new ConcurrentDictionary<string, string>();
            user.BankLinks ??= new List<BankLink>();
            var client = Configuration.BankClient;
            Parallel.ForEach(user.BankLinks, new ParallelOptions { MaxDegreeOfParallelism = 10 }, (bankLink) =>
            {
                try
                {
                    var accountBalance = client.GetAccountBalance(bankLink.AccessToken);
                    institutionsDictionary.TryAdd(
                        accountBalance.Item["institution_id"].Value<string>(),
                        accountBalance.Item["institution_id"].Value<string>());
                    accounts.Add(accountBalance);
                }
                catch (Exception e)
                {
                    failedAccounts.Add(new FinanceUserBankAccountFailedAccount
                    {
                        ItemId = bankLink.ItemId,
                        ErrorDescription = e.ToString(),
                        InstitutionName = bankLink.InstitutionName
                    });
                }
            });
            var institutionDetails = new ConcurrentBag<JObject>();
            Parallel.ForEach(institutionsDictionary.Keys, new ParallelOptions { MaxDegreeOfParallelism = 10 }, institution =>
            {
                institutionDetails.Add(client.GetInstitution(institution));
            });
            var institutionsJson = new JArray();
            foreach (var institutionDetail in institutionDetails)
            {
                institutionsJson.Add(institutionDetail["institution"]);
            }
            foreach (var account in accounts)
            {
                var institutionId = account.Item["institution_id"].Value<string>();
                account.Item["institution"] = institutionsJson.First(x =>
                    string.Equals(x["institution_id"].Value<string>(), institutionId, StringComparison.OrdinalIgnoreCase));
            }
            var financeUserBankAccount = new FinanceUserBankAccount
            {
                Email = user.Email,
                AllAccounts = JArray.FromObject(accounts),
                FailedAccounts = failedAccounts,
                Updated = DateTime.UtcNow.ToString("O")
            };
            userBankAccountClient.Create(financeUserBankAccount);
            return financeUserBankAccount;
        }
    }
}
