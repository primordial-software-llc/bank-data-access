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
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());
            var customers = qboClient.QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);

            var vendorDataClient = new DatabaseClient<Vendor>(new AmazonDynamoDBClient());
            var vendors = vendorDataClient.GetAll()
                .ToDictionary(x => x.QuickBooksOnlineId);

            var activeCustomerPaymentSettings = new List<CustomerPaymentSettingsModel>();
            foreach (var customer in customers.Values)
            {
                vendors.TryGetValue(customer.Id, out var vendor);
                activeCustomerPaymentSettings.Add(new CustomerPaymentSettingsModel
                {
                    Customer = customer,
                    Vendor = vendor
                });
            }
            
            response.Body = JsonConvert.SerializeObject(activeCustomerPaymentSettings);
        }
    }
}
