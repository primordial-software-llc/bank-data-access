using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetLocation : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/location";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<Location>(new AmazonDynamoDBClient(), new ConsoleLogger());
            var locations = databaseClient.ScanAll(new ScanRequest(new Location().GetTable()));
            response.Body = JsonConvert.SerializeObject(locations);
        }
    }
}
