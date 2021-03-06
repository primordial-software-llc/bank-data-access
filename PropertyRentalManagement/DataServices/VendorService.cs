﻿using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement.DataServices
{
    public class VendorService
    {
        public IAmazonDynamoDB DbClient { get; }

        public VendorService(IAmazonDynamoDB dbClient)
        {
            DbClient = dbClient;
        }

        public static Vendor CreateModel(
            int? quickBooksOnlineId,
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

        public List<Vendor> GetByPaymentFrequency(RecurringInvoices.Frequency paymentFrequency)
        {
            if (paymentFrequency != RecurringInvoices.Frequency.Weekly &&
                paymentFrequency != RecurringInvoices.Frequency.Monthly)
            {
                throw new Exception($"Unknown payment frequency {paymentFrequency}");
            }
            var scanRequest = new ScanRequest(new Vendor().GetTable())
            {
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":paymentFrequency", new AttributeValue {S = paymentFrequency == RecurringInvoices.Frequency.Weekly ? "weekly" : "monthly"}}
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#paymentFrequency", "paymentFrequency" }
                },
                FilterExpression = "#paymentFrequency = :paymentFrequency"
            };

            ScanResponse scanResponse = null;
            var items = new List<Vendor>();
            do
            {
                if (scanResponse != null)
                {
                    scanRequest.ExclusiveStartKey = scanResponse.LastEvaluatedKey;
                }
                scanResponse = DbClient.ScanAsync(scanRequest).Result;
                foreach (var item in scanResponse.Items)
                {
                    items.Add(JsonConvert.DeserializeObject<Vendor>(Document.FromAttributeMap(item).ToJson()));
                }
            } while (scanResponse.LastEvaluatedKey.Any());

            return items;
        }
    }
}
