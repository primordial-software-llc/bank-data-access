using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.DatabaseModel;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class PostVendorLocation : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/vendor/location";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            List<string> errors = new List<string>();
            var dbClient = new AmazonDynamoDBClient();
            var logger = new ConsoleLogger();
            var vendorLocationUpdates = JsonConvert.DeserializeObject<List<VendorLocation>>(request.Body);
            if (!vendorLocationUpdates.Any())
            {
                errors.Add("A vendor location is required.");
            }
            if (vendorLocationUpdates.Any(x => string.IsNullOrWhiteSpace(x.LocationId)))
            {
                errors.Add("locationId is required for each vendor location.");
            }
            if (vendorLocationUpdates.Any(x => string.IsNullOrWhiteSpace(x.LocationId)))
            {
                errors.Add("vendorId is required for each vendor location.");
            }
            if (errors.Any())
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", JArray.FromObject(errors) } }.ToString();
                return;
            }

            var locationClient = new DatabaseClient<Location>(dbClient, logger);
            var locations = locationClient.ScanAll(new ScanRequest(new Location().GetTable()));
            var firstVendorLocation = vendorLocationUpdates.First();
            if (!vendorLocationUpdates.All(x =>
                string.Equals(x.VendorId, firstVendorLocation.VendorId, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add("POST must be for a single vendor.");
            }
            var vendorClient = new DatabaseClient<Vendor>(dbClient, logger);
            var vendor = vendorClient.Get(new Vendor { Id = firstVendorLocation.VendorId }).Result;
            if (vendor == null)
            {
                errors.Add($"vendorId {firstVendorLocation.VendorId} doesn't exist.");
            }
            foreach (var vendorLocation in vendorLocationUpdates)
            {
                var location = locationClient.Get(new Location { Id = vendorLocation.LocationId }).Result;
                if (locations.All(x => location.Id != vendorLocation.LocationId))
                {
                    errors.Add($"locationId doesn't exist.");
                }
            }
            if (errors.Any())
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", JArray.FromObject(errors) } }.ToString();
                return;
            }

            var vendorLocationClient = new DatabaseClient<VendorLocation>(dbClient, logger);
            foreach (var location in locations.Where(x => vendorLocationUpdates.All(y => x.Id != y.LocationId)))
            {
                vendorLocationClient.Delete(new VendorLocation { VendorId = vendor.Id, LocationId = location.Id });
            }
            foreach (var vendorLocation in vendorLocationUpdates)
            {
                vendorLocationClient.Create(new VendorLocation
                {
                    VendorId = vendorLocation.VendorId,
                    LocationId = vendorLocation.LocationId,
                    Created = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                    CreatedBy = user.Email
                });
            }

            response.Body = JsonConvert.SerializeObject(vendorLocationUpdates);
        }
    }
}
