using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetRentalSections : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/rental-sections";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<Section>(new AmazonDynamoDBClient());
            response.Body = JsonConvert.SerializeObject(databaseClient.GetAll());
        }
    }
}
