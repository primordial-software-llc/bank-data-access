using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.PlaidModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated
{
    class PostLinkAccessToken : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/link-access-token";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var model = JsonConvert.DeserializeObject<PlaidLinkResponse>(request.Body);
            var client = new BankAccessClient(Configuration.PlaidUrl, new Logger());
            var accessToken = client.GetAccessToken(model.PublicToken);
            var updatedBankLinks = user.BankLinks ?? new List<BankLink>();
            updatedBankLinks.Add(new BankLink
            {
                AccessToken = accessToken.AccessToken,
                ItemId = accessToken.ItemId
            });
            var update = new JObject { { "bankLinks", JToken.FromObject(updatedBankLinks) } };
            var updateItemResponse = new UserService().UpdateUser(user.Email, update);
            response.StatusCode = (int)updateItemResponse.HttpStatusCode;
            response.Body = Constants.JSON_EMPTY;
        }
    }
}
