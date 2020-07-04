using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Vendor : IModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("quickBooksOnlineId")]
        public int QuickBooksOnlineId { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("paymentFrequency")]
        public string PaymentFrequency { get; set; }

        [JsonProperty("rentPrice")]
        public decimal? RentPrice { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = Id } } };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-vendors";
        }
    }
}
