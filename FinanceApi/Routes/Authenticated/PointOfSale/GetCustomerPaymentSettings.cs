using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetCustomerPaymentSettings : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/customer-payment-settings";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var dbClient = new AmazonDynamoDBClient();
            var qboDbClient = new DatabaseClient<QuickBooksOnlineConnection>(dbClient);
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, qboDbClient, new Logger());
            var customers = qboClient.QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);

            var vendorDataClient = new DatabaseClient<Vendor>(dbClient);
            var vendors = vendorDataClient.GetAll()
                .Where(x => customers.ContainsKey(x.QuickBooksOnlineId))
                .ToDictionary(x => x.QuickBooksOnlineId);

            var newCustomers = customers.Where(x => !vendors.ContainsKey(x.Key));
            var vendorService = new VendorService();
            foreach (var newCustomer in newCustomers)
            {
                var vendor = vendorService.CreateModel(newCustomer.Key, null, null, null);
                vendorDataClient.Create(vendor);
                vendors.Add(vendor.QuickBooksOnlineId, vendor);
            }

            var json = new List<CustomerPaymentSettingsModel>();
            foreach (var vendor in vendors.Values)
            {
                var customer = customers[vendor.QuickBooksOnlineId];
                json.Add(new CustomerPaymentSettingsModel
                {
                    Id = vendor.Id,
                    QuickBooksOnlineId = vendor.QuickBooksOnlineId,
                    PaymentFrequency = vendor.PaymentFrequency,
                    RentPrice = vendor.RentPrice,
                    Memo = vendor.Memo,
                    FirstName = customer.GivenName,
                    LastName = customer.FamilyName,
                    DisplayName = customer.DisplayName
                });
            }

            response.Body = JsonConvert.SerializeObject(json);
        }
    }
}
