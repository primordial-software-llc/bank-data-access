using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class PostReceipt : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/receipt";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            if (!new PointOfSaleAuthorization().IsAuthorized(user.Email))
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", "unknown email" } }.ToString();
                return;
            }
            var receipt = JsonConvert.DeserializeObject<Receipt>(request.Body);
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var receiptDbClient = new DatabaseClient<Receipt>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());
            var receiptService = new ReceiptService(receiptDbClient, qboClient);
            var receiptResult = receiptService.SaveReceipt(receipt);
            response.Body = JsonConvert.SerializeObject(receiptResult);
        }
    }
}
