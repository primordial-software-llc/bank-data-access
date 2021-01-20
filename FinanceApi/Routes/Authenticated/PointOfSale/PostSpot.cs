using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class PostSpot : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/spot";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var model = JsonConvert.DeserializeObject<Spot>(request.Body);
            var databaseClient = new DatabaseClient<Spot>(new AmazonDynamoDBClient(), new ConsoleLogger());
            var updated = databaseClient.Create(model);
            response.Body = JsonConvert.SerializeObject(updated);
        }
    }
}
