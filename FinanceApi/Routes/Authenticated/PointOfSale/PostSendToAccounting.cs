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
    class PostSendToAccounting : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/point-of-sale/send-to-accounting";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(new AmazonDynamoDBClient());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, new Logger());
            var accountingClient = new AccountingQuickBooksOnlineClient(qboClient);
            var journal = new PrivateAccountingJournal<JournalEntry>(new AmazonDynamoDBClient(), accountingClient);
            var results = journal.SendToAccounting();
            response.Body = JsonConvert.SerializeObject(results);
        }
    }
}
