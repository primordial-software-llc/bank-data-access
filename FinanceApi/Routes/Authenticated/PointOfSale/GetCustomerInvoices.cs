using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetCustomerInvoices : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/customer-invoices";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var dbClient = new AmazonDynamoDBClient();
            var qboDbClient = new DatabaseClient<QuickBooksOnlineConnection>(dbClient, new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(PropertyRentalManagement.Constants.RealmId, qboDbClient, new ConsoleLogger());
            var vendorDataClient = new DatabaseClient<Vendor>(dbClient, new ConsoleLogger());
            var vendor = vendorDataClient.Get(new Vendor { Id = request.QueryStringParameters["id"] });
            var start = request.QueryStringParameters["start"];
            var end = request.QueryStringParameters["end"];
            var invoices = qboClient.QueryAll<Invoice>($"select * from Invoice where CustomerRef = '{vendor.QuickBooksOnlineId}' and TxnDate >= '{start}' and TxnDate <= '{end}' ORDERBY TxnDate DESC");
            response.Body = JsonConvert.SerializeObject(invoices);
        }
    }
}
