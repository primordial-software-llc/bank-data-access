using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetInvoiceItems : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/invoice-items";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            string locationId = string.Empty;
            if (request.QueryStringParameters != null)
            {
                request.QueryStringParameters.TryGetValue("locationId", out locationId);
            }
            if (string.IsNullOrWhiteSpace(locationId))
            {
                locationId = PropertyRentalManagement.Constants.LOCATION_ID_LAKELAND;
            }

            var dbClient = new AmazonDynamoDBClient();

            var locationClient = new DatabaseClient<Location>(dbClient, new ConsoleLogger());
            var location = locationClient.Get(new Location { Id = locationId }).Result;

            var qboDbClient = new DatabaseClient<QuickBooksOnlineConnection>(dbClient, new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, qboDbClient, new ConsoleLogger());
            var invoiceItemsForLocation = qboClient
                .QueryAll<Item>("item", "Id", location.InvoiceItems)
                .OrderByDescending(x => x.Id == location.RentProductId);

            response.Body = JsonConvert.SerializeObject(invoiceItemsForLocation);
        }
    }
}
