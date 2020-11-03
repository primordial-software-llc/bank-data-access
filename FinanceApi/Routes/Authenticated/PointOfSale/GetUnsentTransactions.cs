using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PrivateAccounting;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using JournalEntry = PropertyRentalManagement.DatabaseModel.JournalEntry;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    class GetUnsentTransactions : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/unsent-transactions";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());
            var accountingClient = new AccountingQuickBooksOnlineClient(qboClient);
            var journal = new PrivateAccountingJournal<JournalEntry>(new AmazonDynamoDBClient(), accountingClient);
            var unsent = journal.GetUnsent();
            response.Body = JsonConvert.SerializeObject(unsent);
        }
    }
}
