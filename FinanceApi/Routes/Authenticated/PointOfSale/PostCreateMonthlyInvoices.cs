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
    public class PostCreateMonthlyInvoices : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/create-monthly-invoices";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());
            var day = JsonConvert.DeserializeObject<CalendarDay>(request.Body);
            var date = new DateTime(day.Year, day.Month, day.DayOfMonth, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(1);
            Console.WriteLine($"{user.Email} is creating monthly invoices for {date:yyyy-MM-dd}");
            var monthlyInvoices = new RecurringInvoices()
                .CreateMonthlyInvoices(
                    date,
                    new VendorService(new AmazonDynamoDBClient()),
                    qboClient);
            Console.WriteLine($"Created {monthlyInvoices.Count} monthly invoices for {JsonConvert.SerializeObject(day)}.");
            foreach (var invoice in monthlyInvoices)
            {
                Console.WriteLine($"Created monthly invoice for {invoice.CustomerRef.Name} - {invoice.CustomerRef.Value}");
            }
            var invoiceJson = JsonConvert.SerializeObject(monthlyInvoices);
            Console.WriteLine(invoiceJson);
            response.Body = invoiceJson;
        }
    }
}
