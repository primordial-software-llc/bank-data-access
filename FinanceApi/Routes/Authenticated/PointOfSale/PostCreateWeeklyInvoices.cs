using System;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class PostCreateWeeklyInvoices : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/create-weekly-invoices";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());
            var day = JsonConvert.DeserializeObject<CalendarDay>(request.Body);
            var date = new DateTime(day.Year, day.Month, day.DayOfMonth, 0, 0, 0, DateTimeKind.Utc);
            Console.WriteLine($"{user.Email} is creating weekly invoices for {date:yyyy-MM-dd}");
            var weeklyInvoices = new RecurringInvoices().CreateWeeklyInvoices(
                date,
                new VendorService(new AmazonDynamoDBClient()),
                qboClient);
            Console.WriteLine($"Created {weeklyInvoices.Count} weekly invoices for {JsonConvert.SerializeObject(day)}");
            foreach (var weeklyInvoice in weeklyInvoices)
            {
                Console.WriteLine($"Created weekly invoice for {weeklyInvoice.CustomerRef.Name} - {weeklyInvoice.CustomerRef.Value}");
            }
            var invoicesJson = JsonConvert.SerializeObject(weeklyInvoices);
            Console.WriteLine(invoicesJson);
            response.Body = invoicesJson;
        }
    }
}
