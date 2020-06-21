using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
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
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());
            var accessTokenJson = client.GetAccessToken(model.PublicToken);
            var updatedBankLinks = user.BankLinks ?? new List<BankLink>();
            updatedBankLinks.Add(new BankLink
            {
                AccessToken = accessTokenJson["access_token"].Value<string>(),
                ItemId = accessTokenJson["item_id"].Value<string>()
            });
            var update = new JObject { { "bankLinks", JToken.FromObject(updatedBankLinks) } };
            var updateItemResponse = new UserService().UpdateUser(user.Email, update);
            response.StatusCode = (int)updateItemResponse.HttpStatusCode;
            response.Body = Constants.JSON_EMPTY;
        }
    }
}
