using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsDataAccess;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Vendor : IModel
    {
        public Vendor()
        {

        }
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("quickBooksOnlineId")]
        public int? QuickBooksOnlineId { get; set; }

        [JsonProperty("paymentFrequency")]
        public string PaymentFrequency { get; set; }

        [JsonProperty("rentPrice")]
        public decimal? RentPrice { get; set; }

        [JsonProperty("memo")]
        public string Memo { get; set; }

        [JsonProperty("spots")]
        public List<Spot> Spots { get; set; }

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
