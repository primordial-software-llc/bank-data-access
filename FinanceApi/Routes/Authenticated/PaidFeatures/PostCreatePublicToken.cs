using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated.PaidFeatures
{
    class PostCreatePublicToken : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/create-public-token";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var model = JsonConvert.DeserializeObject<CreatePublicTokenModel>(request.Body);
            var link = (user.BankLinks ?? new List<BankLink>())
                .First(x => string.Equals(x.ItemId, model.ItemId, StringComparison.OrdinalIgnoreCase));
            var updateResponse = Configuration.BankClient.CreatePublicToken(link.AccessToken);
            var client = Configuration.BankClient; // Get the account balance and add it to the cache. Getting account balances incurs fees from plaid (potential for abuse) and is slow so update the cache.
            var item = client.GetItem(link.AccessToken)["item"];
            var institutionResponse = client.GetInstitution(item["institution_id"].Value<string>());
            var accountBalance = Configuration.BankClient.GetAccountBalance(link.AccessToken);
            accountBalance.Item["institution"] = institutionResponse["institution"];
            var userBankAccountClient = new DatabaseClient<FinanceUserBankAccount>(new AmazonDynamoDBClient(), new ConsoleLogger());
            var userBankAccount = userBankAccountClient.Get(new FinanceUserBankAccount { Email = user.Email });
            userBankAccount.AllAccounts.Add(JObject.FromObject(accountBalance));
            var newFailedAccounts = userBankAccount
                .FailedAccounts
                .Where(x => !string.Equals(x.ItemId, item["item_id"].Value<string>(), StringComparison.OrdinalIgnoreCase)).ToList();
            var bankAccountUpdate = new FinanceUserBankAccount
            {
                Email = userBankAccount.Email,
                AllAccounts = userBankAccount.AllAccounts,
                FailedAccounts = newFailedAccounts,
                Updated = DateTime.UtcNow.ToString("O")
            };
            userBankAccountClient.Update(bankAccountUpdate);

            response.Body = updateResponse.ToString();
        }
    }
}
