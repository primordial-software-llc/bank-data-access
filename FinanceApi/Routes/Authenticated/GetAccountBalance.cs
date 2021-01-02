using System;
using System.Globalization;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.BusinessLogic;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DataServices;

namespace FinanceApi.Routes.Authenticated
{
    class GetAccountBalance : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/accountBalance";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var userBankAccountClient = new DatabaseClient<FinanceUserBankAccount>(new AmazonDynamoDBClient());
            var userBankAccount = userBankAccountClient.Get(new FinanceUserBankAccount { Email = user.Email });
            if (!DateTime.TryParseExact(userBankAccount.Updated, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var updated))
            {
                updated = DateTime.UtcNow.AddYears(-1);
            }
            var refreshPoint = DateTime.UtcNow.AddHours(-1);
            if (refreshPoint <= updated)
            {
                response.Body = JsonConvert.SerializeObject(userBankAccount);
                Console.WriteLine("Returning account data from cache");
                return;
            }
            var financeUserBankAccount = new BankAggregator();
            response.Body = JsonConvert.SerializeObject(financeUserBankAccount);
        }
    }
}
