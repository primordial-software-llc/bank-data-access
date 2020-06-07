using System;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement;
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
            if (
                !string.Equals(user.Email, "timg456789@yahoo.com", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.Email, "timothy@primordial-software.com", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.Email, "kiara@primordial-software.com", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.Email, "lakeland.mipueblo@outlook.com", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(user.Email, "taniagkocher@gmail.com", StringComparison.OrdinalIgnoreCase)
            )
            {
                response.StatusCode = 400;
                response.Body = new JObject {{"error", "unknown email"}}.ToString();
                return;
            }
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());
            var customers = qboClient.QueryAll<Customer>("select * from customer");
            response.Body = JsonConvert.SerializeObject(customers);
        }
    }
}