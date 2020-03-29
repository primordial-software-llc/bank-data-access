using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.PlaidModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated
{
    class GetAccountBalance : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/accountBalance";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var userBankAccountClient = new DatabaseClient<FinanceUserBankAccount>(new AmazonDynamoDBClient());
            var userBankAccount = userBankAccountClient.Get(new FinanceUserBankAccount { Email = user.Email });
            if (!DateTime.TryParseExact(userBankAccount.Updated, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updated))
            {
                updated = DateTime.UtcNow.AddYears(-1);
            }
            var refreshPoint = DateTime.UtcNow.AddHours(-1);
            if (refreshPoint <= updated)
            {
                response.Body = JsonConvert.SerializeObject(userBankAccount);
                Console.WriteLine("Returning account data from cache");
                return;
            }
            Console.WriteLine("Getting account data from plaid");
            var failedAccounts = new JArray();
            var accounts = new ConcurrentBag<AccountBalance>();
            var institutionsDictionary = new ConcurrentDictionary<string, string>();
            user.BankLinks = user.BankLinks ?? new List<BankLink>();
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());
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
                    failedAccounts.Add(new JObject
                    {
                        { "itemId", bankLink.ItemId },
                        { "errorDescription",  e.ToString()}
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
            response.Body = JsonConvert.SerializeObject(financeUserBankAccount);
        }
    }
}
