using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;

namespace FinanceApi.Routes.Authenticated
{
    class GetBudget : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/budget";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            user.BankLinks = null;
            response.Body = JsonConvert.SerializeObject(user);
        }
    }
}
