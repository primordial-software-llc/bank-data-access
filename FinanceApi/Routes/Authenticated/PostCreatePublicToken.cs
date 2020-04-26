using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;

namespace FinanceApi.Routes.Authenticated
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
            var client = new BankAccessClient(Configuration.PlaidUrl, new Logger());
            response.Body = client.CreatePublicToken(link.AccessToken).ToString();
        }
    }
}
