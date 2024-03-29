﻿using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            if (string.IsNullOrWhiteSpace(receipt.LocationId))
            {
                receipt.LocationId = PropertyRentalManagement.Constants.LOCATION_ID_LAKELAND; // WARNING: location id should come from the UI and be required
            }
            var dbClient = new AmazonDynamoDBClient();
            var logger = new ConsoleLogger();
            var locationClient = new DatabaseClient<Location>(dbClient, new ConsoleLogger());
            var location = locationClient.Get(new Location { Id = receipt.LocationId }).Result;
            if (string.IsNullOrWhiteSpace(receipt.InvoiceItem?.Id))
            {
                receipt.InvoiceItem = new PropertyRentalManagement.DatabaseModel.Reference { Id = location.RentProductId.ToString() };
            }
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
            var cardPayment = new CardPayment(logger, Configuration.CLOVER_MI_PUEBLO_PRIVATE_TOKEN);
            var receiptService = new ReceiptSave(receiptDbClient, qboClient, spotReservationDbClient, logger, cardPayment, location);
            string customerId = receipt.Customer.Id;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                var customer = new Customer { DisplayName = receipt.Customer.Name };
                customer = qboClient.Create(customer);
                customerId = customer.Id.ToString();
            }
            var vendor = activeVendors.FirstOrDefault(x => x.QuickBooksOnlineId.GetValueOrDefault().ToString() == customerId);
            if (vendor == null)
            {
                var vendorService = new VendorService(dbClient, logger);
                var vendorResult = vendorService.CreateVendor(int.Parse(customerId), null, null, null, receipt.LocationId);
                vendor = vendorResult.Item1;
            }
            var receiptResult = receiptService.SaveReceipt(receipt, customerId, user.FirstName, user.LastName, user.Email, vendor, IpLookup.GetIp(request), receipt.InvoiceItem.Id);
            response.Body = JsonConvert.SerializeObject(receiptResult);
        }
    }
}
