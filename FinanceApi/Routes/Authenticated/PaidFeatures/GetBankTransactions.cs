using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.BusinessLogic;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;

namespace FinanceApi.Routes.Authenticated.PaidFeatures
{
    public class GetBankTransactions : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/bank-transactions";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var startDate = request.QueryStringParameters != null &&
                            request.QueryStringParameters.ContainsKey("startDate") 
                ? request.QueryStringParameters["startDate"]
                : string.Empty;
            var endDate = request.QueryStringParameters != null &&
                          request.QueryStringParameters.ContainsKey("endDate")
                ? request.QueryStringParameters["endDate"]
                : string.Empty;
            var transactions = new TransactionAggregator()
                .GetTransactions(
                    Configuration.BankClient,
                    user,
                    startDate,
                    endDate);
            response.Body = JsonConvert.SerializeObject(transactions);
        }
    }
}
