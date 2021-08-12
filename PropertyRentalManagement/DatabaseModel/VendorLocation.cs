using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsDataAccess;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class VendorLocation : IModel
    {
        [JsonProperty("locationId")]
        public string LocationId { get; set; }

        [JsonProperty("vendorId")]
        public string VendorId { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue>
            {
                { "locationId", new AttributeValue { S = LocationId } },
                { "vendorId", new AttributeValue { S = VendorId } }
            };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-vendor-location";
        }
    }
}
