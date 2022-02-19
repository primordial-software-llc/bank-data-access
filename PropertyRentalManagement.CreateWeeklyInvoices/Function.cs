using System;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using AwsDataAccess;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))] // Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
namespace PropertyRentalManagement.CreateWeeklyInvoices
{
    public class Function
    {
        public string FunctionHandler(ILambdaContext context)
        {
            var logger = new ConsoleLogger();
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient(), logger);
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, databaseClient, logger);
            var recurringInvoices = new RecurringInvoices(new VendorService(new AmazonDynamoDBClient(), logger), qboClient, logger);
            recurringInvoices.CreateInvoicesForFrequency(DateTime.UtcNow.Date, RecurringInvoices.Frequency.Weekly);
            Console.WriteLine("Scheduled function PropertyRentalManagement.CreateWeeklyInvoices completed");
            return "Scheduled function PropertyRentalManagement.CreateWeeklyInvoices completed";
        }
    }
}
