using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Receipt : IModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("rentalDate")]
        public string RentalDate { get; set; }

        [JsonProperty("customerId")]
        public int CustomerId { get; set; }
        
        [JsonProperty("amountOfAccount")]
        public decimal? AmountOfAccount { get; set; }
        
        [JsonProperty("rentalAmount")]
        public decimal? RentalAmount { get; set; }
        
        [JsonProperty("thisPayment")]
        public decimal? ThisPayment { get; set; }
        
        [JsonProperty("memo")]
        public string Memo { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = Id } } };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-receipts";
        }
    }
}
