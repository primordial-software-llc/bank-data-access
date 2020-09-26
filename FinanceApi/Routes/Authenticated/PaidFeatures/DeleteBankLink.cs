using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated.PaidFeatures
{
    class DeleteBankLink : IRoute
    {
        public string HttpMethod => "DELETE";
        public string Path => "/bank-link";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var model = JsonConvert.DeserializeObject<DeleteBankLinkModel>(request.Body);
            var link = user.BankLinks.First(x =>string.Equals(x.ItemId, model.ItemId, StringComparison.OrdinalIgnoreCase));
            var bankClient = Configuration.BankClient;
            bankClient.RemoveItem(link.AccessToken);
            user.BankLinks.Remove(link);
            var update = new JObject { { "bankLinks", JToken.FromObject(user.BankLinks) } };
            var updateItemResponse = new UserService().UpdateUser(user.Email, update);
            response.StatusCode = (int)updateItemResponse.HttpStatusCode;
            response.Body = Constants.JSON_EMPTY;
        }
    }
}
