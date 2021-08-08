using System;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
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
            var awsDbClient = new AmazonDynamoDBClient();
            var logger = new ConsoleLogger();
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(awsDbClient, logger);
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, databaseClient, new ConsoleLogger());
            var day = JsonConvert.DeserializeObject<CalendarDay>(request.Body);
            var date = new DateTime(day.Year, day.Month, day.DayOfMonth, 0, 0, 0, DateTimeKind.Utc);
            Console.WriteLine($"{user.Email} is creating weekly invoices for {date:yyyy-MM-dd}");
            var taxRate = new Tax().GetTaxRate(qboClient, PropertyRentalManagement.Constants.QUICKBOOKS_TAX_RATE_POLK_COUNTY_RENTAL);
            var recurringInvoices = new RecurringInvoices(new VendorService(awsDbClient), qboClient, taxRate, logger);
            var weeklyInvoices = recurringInvoices.CreateInvoicesForFrequency(date, RecurringInvoices.Frequency.Weekly);
            var invoicesJson = JsonConvert.SerializeObject(weeklyInvoices);
            Console.WriteLine(invoicesJson);
            response.Body = invoicesJson;
        }
    }
}
