using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Unauthenticated
{
    class SetToken : IRoute
    {
        public APIGatewayProxyResponse Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user = null)
        {
            var body = JObject.Parse(request.Body);
            var expirationDate = DateTime.UtcNow.AddDays(30).ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");
            response.MultiValueHeaders = new Dictionary<string, IList<string>>
            {
                {
                    "Set-Cookie", new List<string>
                    {
                        $"idToken={body["idToken"]};Path=/;Secure;HttpOnly;SameSite=Strict;Expires={expirationDate}",
                        $"refreshToken={body["refreshToken"]};Path=/;Secure;HttpOnly;SameSite=Strict;Expires={expirationDate}"
                    }
                }
            };
            response.Body = new JObject().ToString();
            return response;
        }
    }
}
