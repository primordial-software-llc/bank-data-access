using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class PostReceipt : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/receipt";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var receipt = JsonConvert.DeserializeObject<Receipt>(request.Body);
            var dbClient = new AmazonDynamoDBClient();
            var logger = new ConsoleLogger();
            var receiptDbClient = new DatabaseClient<ReceiptSaveResult>(dbClient, logger);
            var spotReservationDbClient = new DatabaseClient<SpotReservation>(dbClient, logger);
            var vendorDbClient = new DatabaseClient<Vendor>(dbClient, logger);
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, new DatabaseClient<QuickBooksOnlineConnection>(dbClient, logger), logger);

            var allActiveCustomers = qboClient
                .QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);
            var activeVendors = new ActiveVendorSearch()
                .GetActiveVendors(allActiveCustomers, vendorDbClient)
                .ToList();
            var spotClient = new DatabaseClient<Spot>(dbClient, logger);
            var spotReservationCheck = new SpotReservationCheck(spotClient, spotReservationDbClient, activeVendors, allActiveCustomers);
            var validation = new ReceiptValidation(spotReservationCheck).Validate(receipt).Result;
            if (validation.Any())
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", JArray.FromObject(validation) } }.ToString();
                return;
            }
            var taxRate = new Tax().GetTaxRate(qboClient, PropertyRentalManagement.Constants.QUICKBOOKS_RENTAL_TAX_RATE);
            var receiptService = new ReceiptSave(receiptDbClient, qboClient, taxRate, spotReservationDbClient);
            string customerId = receipt.Customer.Id;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                var customer = new Customer { DisplayName = receipt.Customer.Name };
                customer = qboClient.Create(customer);
                customerId = customer.Id.ToString();
            }
            var vendor = activeVendors.FirstOrDefault(x => x.QuickBooksOnlineId.GetValueOrDefault().ToString() == customerId)
                         ?? vendorDbClient.Create(VendorService.CreateModel(int.Parse(customerId), null, null, null));
            var receiptResult = receiptService.SaveReceipt(receipt, customerId, user.FirstName, user.LastName, user.Email, vendor);
            response.Body = JsonConvert.SerializeObject(receiptResult);
        }
    }
}
