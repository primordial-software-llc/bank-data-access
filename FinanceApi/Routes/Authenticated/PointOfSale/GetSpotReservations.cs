using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetSpotReservations : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/spot-reservations";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var validation = new List<string>();
            var rentalDate = request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey("rentalDate")
                ? request.QueryStringParameters["rentalDate"]
                : string.Empty;
            validation.AddRange(ReceiptValidation.GetRentalDateValidation(rentalDate));
            if (validation.Any())
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", JArray.FromObject(validation) } }.ToString();
                return;
            }
            var queryRequest = new QueryRequest(new SpotReservation().GetTable())
            {
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":rentalDate", new AttributeValue {S = rentalDate}}
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#rentalDate", "rentalDate" }
                },
                KeyConditionExpression = "#rentalDate = :rentalDate"
            };
            var databaseClient = new DatabaseClient<SpotReservation>(new AmazonDynamoDBClient());
            var spotReservations = databaseClient.QueryAll(queryRequest);
            response.Body = JsonConvert.SerializeObject(spotReservations);
        }
    }
}
