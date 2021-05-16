using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;

namespace FinanceApi.Routes.Authenticated.PointOfSale.Reports
{
    public class GetCashBasisIncomeFromReceipts : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/cash-basis-income-from-receipts";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var start = request.QueryStringParameters["start"];
            var end = request.QueryStringParameters["end"];
            var client = new AwsDataAccess.DatabaseClient<Receipt>(
                new AmazonDynamoDBClient(),
                new ConsoleLogger());
            var scanRequest = new ScanRequest
            {
                TableName = new Receipt().GetTable(),
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":start", new AttributeValue {S = start}},
                    {":end", new AttributeValue {S = end}}
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    {"#timestamp", "timestamp"}
                },
                FilterExpression = "#timestamp between :start and :end"
            };
            var results = client.ScanAll(scanRequest);
            results = results.OrderBy(x => x.Timestamp).ToList();
            response.Body = JsonConvert.SerializeObject(results);
        }
    }
}
