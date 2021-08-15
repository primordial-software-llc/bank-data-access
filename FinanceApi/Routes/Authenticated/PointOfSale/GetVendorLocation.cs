using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;

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
            var locationClient = new DatabaseClient<Location>(dbClient, logger);
            var locations = locationClient.ScanAll(new ScanRequest(new Location().GetTable()));
            var vendorLocationClient = new DatabaseClient<VendorLocation>(dbClient, logger);
            var vendorLocations = new List<VendorLocation>();
            foreach (var location in locations)
            {
                var vendorLocation = vendorLocationClient.Get(new VendorLocation
                {
                    VendorId = vendorId,
                    LocationId = location.Id
                }).Result;
                if (vendorLocation != null)
                {
                    vendorLocations.Add(vendorLocation);
                }
            }
            response.Body = JsonConvert.SerializeObject(vendorLocations);
        }
    }
}
