using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement.DataServices
{
    public class VendorService
    {
        public Vendor CreateModel(
            int quickBooksOnlineId,
            string paymentFrequency,
            decimal? rentPrice,
            string memo)
        {
            var vendor = new Vendor
            {
                Id = Guid.NewGuid().ToString(),
                QuickBooksOnlineId = quickBooksOnlineId,
                PaymentFrequency = paymentFrequency,
                RentPrice = rentPrice,
                Memo = memo
            };
            return vendor;
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
