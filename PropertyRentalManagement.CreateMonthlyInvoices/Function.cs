using System;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using AwsDataAccess;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace PropertyRentalManagement.CreateMonthlyInvoices
{
    public class Function
    {
        public string FunctionHandler(ILambdaContext context)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient(), new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(Constants.RealmId, databaseClient, new ConsoleLogger());
            var taxRate = new Tax().GetTaxRate(qboClient, Constants.QUICKBOOKS_RENTAL_TAX_RATE);
            var recurringInvoices = new RecurringInvoices(new VendorService(new AmazonDynamoDBClient()), qboClient, taxRate, new ConsoleLogger());
            recurringInvoices.CreateInvoicesForFrequency(DateTime.UtcNow.Date, RecurringInvoices.Frequency.Monthly);
            Console.WriteLine("Scheduled function PropertyRentalManagement.CreateMonthlyInvoices completed");
            return "Scheduled function PropertyRentalManagement.CreateMonthlyInvoices completed";
        }
    }
}
