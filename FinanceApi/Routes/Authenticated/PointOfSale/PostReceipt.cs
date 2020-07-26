using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
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
            var receipt = JsonConvert.DeserializeObject<Receipt>(request.Body);
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var receiptDbClient = new DatabaseClient<ReceiptSaveResult>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());

            var validation = new ReceiptValidation().Validate(receipt);
            if (validation.Any())
            {
                response.Body = new JObject { { "error", JArray.FromObject(validation) } }.ToString();
                return;
            }

            var receiptService = new ReceiptSave(receiptDbClient, qboClient, Configuration.POLK_COUNTY_RENTAL_SALES_TAX_RATE);
            var receiptResult = receiptService.SaveReceipt(receipt, user.FirstName, user.LastName, user.Email);
            response.Body = JsonConvert.SerializeObject(receiptResult);
        }
    }
}
