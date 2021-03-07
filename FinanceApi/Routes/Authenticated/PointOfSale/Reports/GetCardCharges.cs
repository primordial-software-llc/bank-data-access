using System;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.BusinessLogic;

namespace FinanceApi.Routes.Authenticated.PointOfSale.Reports
{
    class GetCardCharges : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/card-charges";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var cardPayment = new CardPayment(new ConsoleLogger(), Configuration.CLOVER_MI_PUEBLO_PRIVATE_TOKEN);
            var start = request.QueryStringParameters.ContainsKey("start") ? request.QueryStringParameters["start"] : string.Empty;
            var end = request.QueryStringParameters.ContainsKey("end") ? request.QueryStringParameters["end"] : string.Empty;
            var income = cardPayment.GetCardCharges(start, end);
            response.Body = JsonConvert.SerializeObject(income);
        }
    }
}

