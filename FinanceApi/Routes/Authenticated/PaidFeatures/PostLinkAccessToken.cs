using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.BusinessLogic;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated.PaidFeatures
{
    class PostLinkAccessToken : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/link-access-token";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var model = JsonConvert.DeserializeObject<LinkAccessTokenModel>(request.Body);
            var client = Configuration.BankClient;
            var exchangeAccessToken = client.GetAccessToken(model.PublicToken);
            var item = client.GetItem(exchangeAccessToken.AccessToken)["item"];
            var institutionResponse = client.GetInstitution(item["institution_id"].Value<string>());
            var updatedBankLinks = user.BankLinks ?? new List<BankLink>();
            updatedBankLinks.Add(new BankLink
            {
                AccessToken = exchangeAccessToken.AccessToken,
                ItemId = exchangeAccessToken.ItemId,
                InstitutionName = institutionResponse["institution"]["name"].Value<string>()
            });
            var update = new JObject { { "bankLinks", JToken.FromObject(updatedBankLinks) } };
            new UserService().UpdateUser(user.Email, update);

            new BankAggregator().GetAndCacheFinanceUserBankAccount(user,
                new DatabaseClient<FinanceUserBankAccount>(new AmazonDynamoDBClient(), new ConsoleLogger()));

            response.StatusCode = 200;
            response.Body = Constants.JSON_EMPTY;
        }
    }
}
