using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Extensions;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class PatchVendor : IRoute
    {
        public string HttpMethod => "PATCH";
        public string Path => "/point-of-sale/vendor";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            List<string> errors = new List<string>();
            var dbClient = new AmazonDynamoDBClient();
            var logger = new ConsoleLogger();
            var spotClient = new DatabaseClient<Spot>(dbClient, logger);
            var vendorDataClient = new DatabaseClient<Vendor>(dbClient, logger);
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(dbClient, logger);
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, logger);
            var vendorUpdates = JsonConvert.DeserializeObject<Vendor>(request.Body);
            if (vendorUpdates.Memo != null && vendorUpdates.Memo.Length > 4000)
            {
                errors.Add("Memo can't exceed 4,000 characters");
            }
            var spotReservationDbClient = new DatabaseClient<SpotReservation>(dbClient, logger);
            var allActiveCustomers = qboClient
                .QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);
            var allActiveVendors = new ActiveVendorSearch()
                .GetActiveVendors(allActiveCustomers, vendorDataClient)
                .ToList();
            var spotReservationCheck = new SpotReservationCheck(spotClient, spotReservationDbClient, allActiveVendors, allActiveCustomers);
            ZonedClock easternClock = SystemClock.Instance.InZone(Configuration.LakeLandMiPuebloTimeZone);
            LocalDate easternToday = easternClock.GetCurrentDate();
            while (easternToday.DayOfWeek != IsoDayOfWeek.Sunday)
            {
                easternToday = easternToday.PlusDays(1);
            }
            string easternRentalDate = easternToday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            errors.AddRange(spotReservationCheck.GetSpotConflicts(vendorUpdates.Spots, easternRentalDate, vendorUpdates.Id).Result);

            if (errors.Any())
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", JArray.FromObject(errors) } }.ToString();
                return;
            }

            var updated = vendorDataClient.Update(vendorUpdates);
            response.Body = JsonConvert.SerializeObject(updated);
        }
    }
}
