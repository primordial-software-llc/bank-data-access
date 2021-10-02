using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated
{
    class GetBudget : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/budget";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var sanitizedUser = JObject.FromObject(user);
            response.Body = JsonConvert.SerializeObject(sanitizedUser);
        }
    }
}
