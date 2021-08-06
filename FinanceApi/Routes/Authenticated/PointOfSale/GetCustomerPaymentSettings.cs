using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetCustomerPaymentSettings : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/customer-payment-settings";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            string includeInactiveCustomersWithSpotsRaw = string.Empty;
            if (request.QueryStringParameters != null)
            {
                request.QueryStringParameters.TryGetValue("includeInactiveCustomersWithSpots", out includeInactiveCustomersWithSpotsRaw);
            }
            bool.TryParse(includeInactiveCustomersWithSpotsRaw, out bool includeInactiveCustomersWithSpots);
            var dbClient = new AmazonDynamoDBClient();
            var qboDbClient = new DatabaseClient<QuickBooksOnlineConnection>(dbClient, new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, qboDbClient, new ConsoleLogger());
            var vendorClient = new DatabaseClient<PropertyRentalManagement.DatabaseModel.Vendor>(dbClient, new ConsoleLogger());
            var nonRentalCustomerIds = PropertyRentalManagement.Constants.NonRentalCustomerIds;
            var allActiveCustomers = qboClient.QueryAll<Customer>("select * from customer")
                .Where(x => !nonRentalCustomerIds.Contains(x.Id.GetValueOrDefault()))
                .ToDictionary(x => x.Id);

            var activeVendors = vendorClient.ScanAll(new ScanRequest(new PropertyRentalManagement.DatabaseModel.Vendor().GetTable()))
                .Where(x =>
                    allActiveCustomers.ContainsKey(x.QuickBooksOnlineId) ||
                    (includeInactiveCustomersWithSpots && x.Spots != null && x.Spots.Any())
                )
                .ToDictionary(x => x.QuickBooksOnlineId);

            var newCustomers = allActiveCustomers.Where(x => !activeVendors.ContainsKey(x.Key));
            foreach (var newCustomer in newCustomers)
            {
                var vendor = VendorService.CreateModel(newCustomer.Key, null, null, null);
                vendorClient.Create(vendor);
                activeVendors.Add(vendor.QuickBooksOnlineId, vendor);
            }

            var json = new List<CustomerPaymentSettingsModel>();
            foreach (var vendor in activeVendors.Values)
            {
                allActiveCustomers.TryGetValue(vendor.QuickBooksOnlineId, out var customer);
                json.Add(new CustomerPaymentSettingsModel
                {
                    Id = vendor.Id,
                    QuickBooksOnlineId = vendor.QuickBooksOnlineId,
                    PaymentFrequency = vendor.PaymentFrequency,
                    RentPrice = vendor.RentPrice,
                    Memo = vendor.Memo,
                    FirstName = customer?.GivenName,
                    LastName = customer?.FamilyName,
                    DisplayName = customer?.DisplayName,
                    Balance = customer?.Balance,
                    Spots = vendor.Spots,
                    isActive = customer != null
                });
            }
            response.Body = JsonConvert.SerializeObject(json);
        }
    }
}
