using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.BusinessLogic;

namespace FinanceApi.Routes.Authenticated.PointOfSale.Reports
{
    class GetCardIncome : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/card-income";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var cardPayment = new CardPayment(new ConsoleLogger(), Configuration.CLOVER_MI_PUEBLO_PRIVATE_TOKEN);
            var start = int.Parse(request.QueryStringParameters["start"]);
            var end = int.Parse(request.QueryStringParameters["end"]);
            var income = cardPayment.GetCardCharges(start, end);
            response.Body = JsonConvert.SerializeObject(income);
        }
    }
}

