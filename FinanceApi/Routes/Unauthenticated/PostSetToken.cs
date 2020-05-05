using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;

namespace FinanceApi.Routes.Unauthenticated
{
    class PostSetToken : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/unauthenticated/setToken";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var model = JsonConvert.DeserializeObject<SetTokenModel>(request.Body);
            var expirationDate = DateTime.UtcNow.AddDays(30).ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");
            var sameSite = Configuration.PlaidUrl.ToLower() == "https://sandbox.plaid.com".ToLower()
                ? "" // Don't worry about exposing test cookies, I don't want to setup a custom domain for the test api's.
                : "SameSite=Strict;";
            response.MultiValueHeaders = new Dictionary<string, IList<string>>
            {
                {
                    "Set-Cookie", new List<string>
                    {
                        $"idToken={model.IdToken};Path=/;Secure;HttpOnly;{sameSite}Expires={expirationDate}",
                        $"refreshToken={model.RefreshToken};Path=/;Secure;HttpOnly;{sameSite}Expires={expirationDate}"
                    }
                }
            };
            response.Body = Constants.JSON_EMPTY;
        }
    }
}
