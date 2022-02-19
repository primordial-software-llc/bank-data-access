using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AwsDataAccess;
using Newtonsoft.Json;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement.DataServices
{
    public class VendorService
    {
        public IAmazonDynamoDB DbClient { get; }
        public ILogging Logger { get; }

        public VendorService(IAmazonDynamoDB dbClient, ILogging logger)
        {
            DbClient = dbClient;
            Logger = logger;
        }

        public Tuple<Vendor, VendorLocation> CreateVendor(
            int? quickBooksOnlineId,
            string paymentFrequency,
            decimal? rentPrice,
            string memo,
            string locationId)
        {
            var vendorClient = new DatabaseClient<Vendor>(DbClient, Logger);
            var vendor = vendorClient.Create(CreateModel(quickBooksOnlineId, paymentFrequency, rentPrice, memo));
            var vendorLocation = AssignVendorLocation(vendor.Id, locationId);
            return new Tuple<Vendor, VendorLocation>(vendor, vendorLocation);
        }

        public VendorLocation AssignVendorLocation(string vendorId, string locationId)
        {
            var vendorLocationClient = new DatabaseClient<VendorLocation>(DbClient, Logger);
            var vendorLocation = vendorLocationClient.Create(new VendorLocation
            {
                VendorId = vendorId,
                LocationId = locationId,
            });
            return vendorLocation;
        }

        private Vendor CreateModel(
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

        public List<Location> GetLocations()
        {
            var locationClient = new DatabaseClient<Location>(DbClient, Logger);
            var locations = locationClient.ScanAll(new ScanRequest(new Location().GetTable()));
            return locations;
        }

        public List<VendorLocation> GetVendorLocations(
            List<Location> locations,
            string vendorId)
        {
            var vendorLocationClient = new DatabaseClient<VendorLocation>(DbClient, Logger);
            var vendorLocations = new List<VendorLocation>();
            foreach (var location in locations)
            {
                var vendorLocation = vendorLocationClient.Get(new VendorLocation
                {
                    VendorId = vendorId,
                    LocationId = location.Id
                }).Result;
                if (vendorLocation != null)
                {
                    vendorLocations.Add(vendorLocation);
                }
            }
            return vendorLocations;
        }
    }
}
