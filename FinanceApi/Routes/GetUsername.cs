using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes
{
    class GetUsername
    {
        public APIGatewayProxyResponse Run(string email, APIGatewayProxyRequest request, APIGatewayProxyResponse response)
        {
            response.Body = new JObject
            {
                { "username", email }
            }.ToString();
            return response;
        }
    }
}
