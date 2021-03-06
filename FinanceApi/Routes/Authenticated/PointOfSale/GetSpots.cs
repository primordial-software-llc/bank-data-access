﻿using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using AwsDataAccess;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetSpots : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/spots";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var databaseClient = new DatabaseClient<Spot>(new AmazonDynamoDBClient(), new ConsoleLogger());
            var spots = databaseClient.ScanAll(new ScanRequest(new Spot().GetTable()))
                .OrderBy(x => x.Section?.Name)
                .ThenBy(x => x.Name)
                .ToList();
            response.Body = JsonConvert.SerializeObject(spots, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}
