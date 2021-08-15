using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetCustomerPaymentSettings : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/customer-payment-settings";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            string includeInactiveCustomersWithSpotsRaw = string.Empty;
            string locationId = string.Empty;
            if (request.QueryStringParameters != null)
            {
                request.QueryStringParameters.TryGetValue("includeInactiveCustomersWithSpots", out includeInactiveCustomersWithSpotsRaw);
                request.QueryStringParameters.TryGetValue("locationId", out locationId);
            }
            if (string.IsNullOrWhiteSpace(locationId))
            {
                locationId = PropertyRentalManagement.Constants.LOCATION_ID_LAKELAND;
            }
            bool.TryParse(includeInactiveCustomersWithSpotsRaw, out bool includeInactiveCustomersWithSpots);
            var dbClient = new AmazonDynamoDBClient();
            var logger = new ConsoleLogger();
            var qboDbClient = new DatabaseClient<QuickBooksOnlineConnection>(dbClient, logger);
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, qboDbClient, logger);
            var vendorClient = new DatabaseClient<Vendor>(dbClient, logger);
            var vendorLocationClient = new DatabaseClient<VendorLocation>(dbClient, logger);
            var getCustomerPaymentSettingsCore = new GetCustomerPaymentSettingsCore(
                qboClient,
                vendorClient,
                vendorLocationClient,
                new VendorService(dbClient, new ConsoleLogger())
            );
            var json = getCustomerPaymentSettingsCore.GetCustomerPaymentSettings(
                includeInactiveCustomersWithSpots,
                locationId);
            response.Body = JsonConvert.SerializeObject(json);
        }
    }
}
