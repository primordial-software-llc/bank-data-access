using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.BusinessLogic;
using FinanceApi.DatabaseModel;
using FinanceApi.PlaidModel;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated.PaidFeatures
{
    class PostCreateLinkToken : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/create-link-token";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var client = Configuration.BankClient;
            var createLinkRequest = new CreateLinkToken
            {
                ClientId = Configuration.PLAID_CLIENT_ID,
                Secret = Configuration.PLAID_SECRET,
                ClientName = "Income Calculator",
                User = new JObject { { "client_user_id", "user.Id" } },
                CountryCodes = new List<string>{ "US" },
                Language = "en"
            };
            if (!string.IsNullOrWhiteSpace(request.Body))
            {
                var model = JObject.Parse(request.Body);
                if (model["itemId"] != null && !string.IsNullOrWhiteSpace(model["itemId"].Value<string>()))
                {
                    var link = (user.BankLinks ?? new List<BankLink>())
                        .First(x => string.Equals(x.ItemId, model["itemId"].Value<string>(), StringComparison.OrdinalIgnoreCase));
                    createLinkRequest.AccessToken = link.AccessToken;
                }
            }
            if (string.IsNullOrWhiteSpace(createLinkRequest.AccessToken))
            {
                createLinkRequest.Products = new List<string> {"transactions"};
            }
            var newLink = client.Send("/link/token/create", createLinkRequest);
            new BankAggregator().GetAndCacheFinanceUserBankAccount(user,
                new DatabaseClient<FinanceUserBankAccount>(new AmazonDynamoDBClient(), new ConsoleLogger()));
            response.StatusCode = 200;
            response.Body = newLink.ToString();
        }
    }
}
