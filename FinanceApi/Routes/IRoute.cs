using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;

namespace FinanceApi.Routes
{
    interface IRoute
    {
        string HttpMethod { get; }
        string Path { get; }
        void Run(
            APIGatewayProxyRequest request,
            APIGatewayProxyResponse response,
            FinanceUser user);
    }
}
