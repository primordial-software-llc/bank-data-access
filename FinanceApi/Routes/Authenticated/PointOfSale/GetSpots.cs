using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetSpots : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/spots";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<Spot>(new AmazonDynamoDBClient());
            var spots = databaseClient.ScanAll(new ScanRequest(new Spot().GetTable()))
                .OrderBy(x => x.Section?.Name)
                .ThenBy(x => x.Name)
                .ToList();
            response.Body = JsonConvert.SerializeObject(spots);
        }
    }
}
