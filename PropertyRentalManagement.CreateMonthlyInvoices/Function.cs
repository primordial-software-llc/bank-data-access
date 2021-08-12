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
            var logger = new ConsoleLogger();
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient(), logger);
            var qboClient = new QuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, databaseClient, logger);
            var taxRate = new Tax().GetTaxRate(qboClient, Constants.QUICKBOOKS_TAX_RATE_POLK_COUNTY_RENTAL);
            var recurringInvoices = new RecurringInvoices(new VendorService(new AmazonDynamoDBClient(), logger), qboClient, taxRate, logger);
            recurringInvoices.CreateInvoicesForFrequency(
                DateTime.UtcNow.Date.AddMonths(1),
                RecurringInvoices.Frequency.Monthly);
            Console.WriteLine("Scheduled function PropertyRentalManagement.CreateMonthlyInvoices completed");
            return "Scheduled function PropertyRentalManagement.CreateMonthlyInvoices completed";
        }
    }
}
