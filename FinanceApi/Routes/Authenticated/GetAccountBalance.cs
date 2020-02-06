using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.PlaidModel;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated
{
    class GetAccountBalance : IRoute
    {
        public APIGatewayProxyResponse Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
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
            var json = new JObject
            {
                {"allAccounts", JArray.FromObject(accounts)},
                {"failedAccounts", failedAccounts }
            };
            response.Body = json.ToString();
            return response;
        }
    }
}
