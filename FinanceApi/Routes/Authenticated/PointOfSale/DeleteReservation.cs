using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class DeleteReservation : IRoute
    {
        public string HttpMethod => "DELETE";
        public string Path => "/point-of-sale/spot-reservation";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var spotReservation = JsonConvert.DeserializeObject<SpotReservation>(request.Body);
            var databaseClient = new DatabaseClient<SpotReservation>(new AmazonDynamoDBClient(), new ConsoleLogger());
            databaseClient.Delete(spotReservation);
            response.StatusCode = 200;
            response.Body = Constants.JSON_EMPTY;
        }
    }
}
