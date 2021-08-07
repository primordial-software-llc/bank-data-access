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
            var customers = qboClient.QueryAll<Customer>("select * from customer")
                .Where(x => !nonRentalCustomerIds.Contains(x.Id.GetValueOrDefault()))
                .ToDictionary(x => x.Id);

            var vendors = vendorClient.ScanAll(new ScanRequest(new PropertyRentalManagement.DatabaseModel.Vendor().GetTable()))
                .Where(x =>
                    customers.ContainsKey(x.QuickBooksOnlineId) ||
                    (includeInactiveCustomersWithSpots && x.Spots != null && x.Spots.Any())
                )
                .ToDictionary(x => x.QuickBooksOnlineId);

            var newCustomers = customers.Where(x => !vendors.ContainsKey(x.Key));
            foreach (var newCustomer in newCustomers)
            {
                var vendor = VendorService.CreateModel(newCustomer.Key, null, null, null);
                vendorClient.Create(vendor);
                vendors.Add(vendor.QuickBooksOnlineId, vendor);
            }

            if (includeInactiveCustomersWithSpots)
            {
                var inactiveVendors = vendors.Where(x => !customers.ContainsKey(x.Key));
                var inactiveCustomerIds = inactiveVendors.Select(x => $"'{x.Value.QuickBooksOnlineId}'");
                var inactiveCustomers = qboClient.Query<Customer>($"select * from customer where Id in ({string.Join(", ", inactiveCustomerIds)}) and Active = false");
                inactiveCustomers.ForEach(x => customers.TryAdd(x.Id, x));
            }

            var json = new List<CustomerPaymentSettingsModel>();
            foreach (var vendor in vendors.Values)
            {
                customers.TryGetValue(vendor.QuickBooksOnlineId, out var customer);
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
                    isActive = (customer?.Active).GetValueOrDefault()
                });
            }
            response.Body = JsonConvert.SerializeObject(json);
        }
    }
}
