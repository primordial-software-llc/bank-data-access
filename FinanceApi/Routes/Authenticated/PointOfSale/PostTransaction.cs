using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrivateAccounting;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using JournalEntry = PropertyRentalManagement.DatabaseModel.JournalEntry;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class PostTransaction : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/transaction";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient(), new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new ConsoleLogger());
            var accountingClient = new AccountingQuickBooksOnlineClient(qboClient);
            var journal = new PrivateAccountingJournal<JournalEntry>(new AmazonDynamoDBClient(), accountingClient);
            var journalEntry = JsonConvert.DeserializeObject<JournalEntry>(request.Body);
            if (string.Equals(journalEntry.Type ?? string.Empty, Accounting.Constants.TYPE_INCOME, StringComparison.OrdinalIgnoreCase))
            {
                journalEntry.TaxCode = PrivateAccounting.Constants.QUICKBOOKS_TAX_RATE;
            }
            var validation = new List<string>();
            validation.AddRange(GetDateValidation(journalEntry.Date));
            if (!string.Equals(journalEntry.Type ?? string.Empty, Accounting.Constants.TYPE_INCOME, StringComparison.Ordinal) &&
                !string.Equals(journalEntry.Type ?? string.Empty, Accounting.Constants.TYPE_EXPENSE, StringComparison.Ordinal))
            {
                validation.Add($"Type is required and must be {Accounting.Constants.TYPE_INCOME} or {Accounting.Constants.TYPE_EXPENSE}");
            }
            if (journalEntry.Amount.GetValueOrDefault() <= 0)
            {
                validation.Add("Amount must be present and greater than 0");
            }
            if (validation.Any())
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", JArray.FromObject(validation) } }.ToString();
                return;
            }
            journal.Save(journalEntry);
            response.Body = new JObject().ToString();
        }

        public static List<string> GetDateValidation(string rentalDate)
        {
            var validation = new List<string>();
            if (string.IsNullOrWhiteSpace(rentalDate))
            {
                validation.Add("Date is required");
            }
            else if (!DateTime.TryParseExact(rentalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedRentalDate))
            {
                validation.Add("Date must be in the format YYYY-MM-DD e.g. 1989-06-16");
            }
            return validation;
        }
    }
}
