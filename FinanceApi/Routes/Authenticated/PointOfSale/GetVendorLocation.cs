using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DataServices;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetVendorLocation : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/vendor/location";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var dbClient = new AmazonDynamoDBClient();
            var logger = new ConsoleLogger();
            var vendorId = request.QueryStringParameters["vendorId"];
            var vendorService = new VendorService(dbClient, logger);
            var locations = vendorService.GetLocations();
            var vendorLocations = vendorService.GetVendorLocations(locations, vendorId);
            response.Body = JsonConvert.SerializeObject(vendorLocations);
        }
    }
}
