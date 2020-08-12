using System.Collections.Generic;
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
            var vendorDataClient = new DatabaseClient<Vendor>(dbClient);
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());

            var vendorUpdates = JsonConvert.DeserializeObject<Vendor>(request.Body);
            var vendorId = vendorUpdates.GetKey();
            vendorUpdates.Id = null;
            if (vendorUpdates.Memo != null && vendorUpdates.Memo.Length > 4000)
            {
                errors.Add("Memo can't exceed 4,000 characters");
            }

            var spotReservationDbClient = new DatabaseClient<SpotReservation>(new AmazonDynamoDBClient());
            var spotReservationCheck = new SpotReservationCheck(spotReservationDbClient, vendorDataClient, qboClient);

            DateTimeZone easternTimeZone = DateTimeZoneProviders.Tzdb["America/New_York"];
            ZonedClock easternClock = SystemClock.Instance.InZone(easternTimeZone);
            LocalDate easternToday = easternClock.GetCurrentDate();
            while (easternToday.DayOfWeek != IsoDayOfWeek.Sunday)
            {
                easternToday = easternToday.PlusDays(1);
            }
            string easternRentalDate = easternToday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            errors.AddRange(spotReservationCheck.GetSpotConflicts(vendorUpdates.Spots, easternRentalDate));

            if (errors.Any())
            {
                response.Body = new JObject { { "error", JArray.FromObject(errors) } }.ToString();
                return;
            }

            var updated = vendorDataClient.Update(vendorId, vendorUpdates);
            response.Body = JsonConvert.SerializeObject(updated);
        }
    }
}
