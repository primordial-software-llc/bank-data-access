using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.ResponseModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace FinanceApi.Routes.Unauthenticated.PointOfSale
{
    public class GetSpotsForPublic : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/unauthenticated/point-of-sale/spots";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<Spot>(new AmazonDynamoDBClient());
            var spots = databaseClient.ScanAll(new ScanRequest(new Spot().GetTable()))
                .OrderBy(x => x.Section?.Name)
                .ThenBy(x => x.Name)
                .ToList();
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient()), new Logger());
            var allActiveCustomers = qboClient
                .QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);
            var allActiveVendors = new ActiveVendorSearch()
                .GetActiveVendors(allActiveCustomers, new DatabaseClient<Vendor>(new AmazonDynamoDBClient()))
                .ToList();
            var spotReservationCheck = new SpotReservationCheck(
                new DatabaseClient<SpotReservation>(new AmazonDynamoDBClient()),
                allActiveVendors,
                allActiveCustomers
            );
            var rentalDate = request.QueryStringParameters.ContainsKey("rentalDate")
                ? request.QueryStringParameters["rentalDate"]
                : string.Empty;
            var validation = ReceiptValidation.GetRentalDateValidation(rentalDate);
            if (validation.Any())
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", JArray.FromObject(validation) } }.ToString();
                return;
            }
            validation = new List<string>();
            DateTime.TryParseExact(rentalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedRentalDate);
            if (parsedRentalDate - DateTime.UtcNow > TimeSpan.FromDays(91)) // Endpoint is cached. The date constraint prevents denial of service causing the cache to be hit and database hits to plateau quickly.
            {
                validation.Add("rentalDate can't be greater than 90 days in the future");
            }
            if (parsedRentalDate - DateTime.UtcNow < TimeSpan.FromDays(-1))
            {
                validation.Add("rentalDate can't be in the past");
            }
            if (validation.Any())
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", JArray.FromObject(validation) } }.ToString();
                return;
            }
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
