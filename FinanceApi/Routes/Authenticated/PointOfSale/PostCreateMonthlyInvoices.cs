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
    public class PostCreateMonthlyInvoices : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/create-monthly-invoices";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient(), new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, databaseClient, new ConsoleLogger());
            var day = JsonConvert.DeserializeObject<CalendarDay>(request.Body);
            var date = new DateTime(day.Year, day.Month, day.DayOfMonth, 0, 0, 0, DateTimeKind.Utc);
            Console.WriteLine($"{user.Email} is creating monthly invoices for {date:yyyy-MM-dd}");
            var taxRate = new Tax().GetTaxRate(qboClient, PropertyRentalManagement.Constants.QUICKBOOKS_TAX_RATE_POLK_COUNTY_RENTAL);
            var recurringInvoices = new RecurringInvoices(new VendorService(new AmazonDynamoDBClient(), new ConsoleLogger()), qboClient, taxRate, new ConsoleLogger());
            var monthlyInvoices = recurringInvoices.CreateInvoicesForFrequency(date, RecurringInvoices.Frequency.Monthly);
            var invoiceJson = JsonConvert.SerializeObject(monthlyInvoices);
            response.Body = invoiceJson;
        }
    }
}
