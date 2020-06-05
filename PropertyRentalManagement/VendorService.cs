using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.DatabaseModel;

namespace Api
{
    public class VendorService
    {
        public void Create(IAmazonDynamoDB dbClient, int quickBooksOnlineId, bool isActive, string paymentFrequency)
        {
            var user = new Vendor
            {
                Id = Guid.NewGuid().ToString(),
                QuickBooksOnlineId = quickBooksOnlineId,
                IsActive = isActive,
                PaymentFrequency = paymentFrequency
            };
            var update = JObject.FromObject(user, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            dbClient.PutItemAsync(
                new Vendor().GetTable(),
                Document.FromJson(update.ToString()).ToAttributeMap()
            ).Wait();
        }

        public List<Vendor> GetByPaymentFrequency(IAmazonDynamoDB dbClient, string paymentFrequency)
        {
            var scanRequest = new ScanRequest(new Vendor().GetTable())
            {
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":paymentFrequency", new AttributeValue {S = paymentFrequency}}
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#paymentFrequency", "paymentFrequency" }
                },
                FilterExpression = "#paymentFrequency = :paymentFrequency"
            };

            ScanResponse scanResponse = null;

            var allMatches = new List<Dictionary<string, AttributeValue>>();
            do
            {
                if (scanResponse != null)
                {
                    scanRequest.ExclusiveStartKey = scanResponse.LastEvaluatedKey;
                }
                scanResponse = dbClient.ScanAsync(scanRequest).Result;
                if (scanResponse.Items.Any())
                {
                    allMatches.AddRange(scanResponse.Items);
                }
            } while (scanResponse.LastEvaluatedKey.Any());

            return Conversion<Vendor>.ConvertToPoco(allMatches);
        }
    }
}
