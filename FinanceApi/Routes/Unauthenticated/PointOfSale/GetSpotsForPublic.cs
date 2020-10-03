using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.ResponseModels;
using Newtonsoft.Json;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace FinanceApi.Routes.Unauthenticated.PointOfSale
{
    public class GetSpotsForPublic
    {
        public string HttpMethod => "POST";
        public string Path => "/unauthenticated/point-of-sale/spots";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<Spot>(new AmazonDynamoDBClient());
            var spots = databaseClient.GetAll()
                .OrderBy(x => x.Section?.Name)
                .ThenBy(x => x.Name)
                .ToList();
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient()), new Logger());
            var allActiveCustomers = qboClient
                .QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);

            var spotReservationCheck = new SpotReservationCheck(
                new DatabaseClient<SpotReservation>(new AmazonDynamoDBClient()),
                new DatabaseClient<Vendor>(new AmazonDynamoDBClient()),
                allActiveCustomers
            );

            var rentalDate = request.QueryStringParameters["rentalDate"];

            var jsonResponse = new List<PublicSpot>();
            foreach (var spot in spots)
            {
                var reservation = spotReservationCheck.GetReservation(spot.Id, rentalDate);
                int? reservedByQuickBooksOnlineId = 0;
                if (reservation?.Item1 != null)
                {
                    reservedByQuickBooksOnlineId = reservation.Item1.QuickBooksOnlineId;
                }
                if (reservation?.Item2 != null)
                {
                    reservedByQuickBooksOnlineId = reservation.Item2.QuickBooksOnlineId;
                }
                var publicSpot = new PublicSpot
                {
                    Id = spot.Id,
                    Name = spot.Name,
                    Section = spot.Section,
                    Right = spot.Right,
                    Bottom = spot.Bottom
                };
                if (reservedByQuickBooksOnlineId.HasValue &&
                    allActiveCustomers.TryGetValue(reservedByQuickBooksOnlineId, out var customer))
                {
                    publicSpot.ReservedBy = customer.DisplayName;
                }
                jsonResponse.Add(publicSpot);
            }
            response.Body = JsonConvert.SerializeObject(jsonResponse);
        }
    }
}
