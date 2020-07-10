using System;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class PatchVendor : IRoute
    {
        public string HttpMethod => "PATCH";
        public string Path => "/point-of-sale/vendor";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var dbClient = new AmazonDynamoDBClient();
            var vendorDataClient = new DatabaseClient<Vendor>(dbClient);

            var vendorUpdates = JsonConvert.DeserializeObject<Vendor>(request.Body);
            var vendorId = vendorUpdates.GetKey();
            vendorUpdates.Id = null;
            if (vendorUpdates.Memo != null && vendorUpdates.Memo.Length > 4000)
            {
                throw new Exception("Memo can't exceed 4,000 characters");
            }
            var updated = vendorDataClient.Update(vendorId, vendorUpdates);

            response.Body = JsonConvert.SerializeObject(updated);
        }
    }
}
