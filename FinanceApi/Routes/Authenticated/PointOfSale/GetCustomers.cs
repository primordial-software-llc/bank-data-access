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
    public class GetCustomers : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/customers";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient(), new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(PropertyRentalManagement.Constants.RealmId, databaseClient, new ConsoleLogger());
            var customers = qboClient.QueryAll<Customer>("select * from customer");
            response.Body = JsonConvert.SerializeObject(customers);
        }
    }
}