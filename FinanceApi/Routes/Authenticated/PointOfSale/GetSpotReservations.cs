using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
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
            var databaseClient = new DatabaseClient<SpotReservation>(new AmazonDynamoDBClient());
            var service = new SpotReservationService(databaseClient);

            List<SpotReservation> spotReservations;
            if (!string.IsNullOrWhiteSpace(rentalDate))
            {
                validation.AddRange(ReceiptValidation.GetRentalDateValidation(rentalDate));
                if (validation.Any())
                {
                    response.StatusCode = 400;
                    response.Body = new JObject { { "error", JArray.FromObject(validation) } }.ToString();
                    return;
                }

                spotReservations = service.GetSpotReservations(rentalDate);
            }
            else
            {
                var vendorId = request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey("vendorId")
                    ? request.QueryStringParameters["vendorId"]
                    : string.Empty;
                spotReservations = service.GetSpotReservationsByVendor(vendorId)
                    .OrderBy(x => x.RentalDate)
                    .ToList();
            }

            response.Body = JsonConvert.SerializeObject(spotReservations);
        }
    }
}
