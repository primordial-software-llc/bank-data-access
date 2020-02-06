using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;

namespace FinanceApi.Routes
{
    interface IRoute
    {
        APIGatewayProxyResponse Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response,
            FinanceUser user = null);
    }
}
