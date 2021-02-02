using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetCustomerPaymentSettingsById : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/customer-payment-settings-by-id";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var dbClient = new AmazonDynamoDBClient();
            var qboDbClient = new DatabaseClient<QuickBooksOnlineConnection>(dbClient, new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, qboDbClient, new ConsoleLogger());

            var vendorDataClient = new DatabaseClient<Vendor>(dbClient, new ConsoleLogger());
            var vendor = vendorDataClient.Get(new Vendor {Id = request.QueryStringParameters["id"]});
            var customer = qboClient.Query<Customer>($"select * from customer where Id = '{vendor.QuickBooksOnlineId}'").First();

            var json = new CustomerPaymentSettingsModel
            {
                Id = vendor.Id,
                QuickBooksOnlineId = vendor.QuickBooksOnlineId,
                PaymentFrequency = vendor.PaymentFrequency,
                RentPrice = vendor.RentPrice,
                Memo = vendor.Memo,
                FirstName = customer.GivenName,
                LastName = customer.FamilyName,
                DisplayName = customer.DisplayName,
                Spots = vendor.Spots
            };

            response.Body = JsonConvert.SerializeObject(json);
        }
    }
}
