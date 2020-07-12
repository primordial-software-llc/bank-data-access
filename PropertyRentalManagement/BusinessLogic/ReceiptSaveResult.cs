using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ReceiptSaveResult : IModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("receipt")]
        public Receipt Receipt { get; set; }
        
        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }

        [JsonProperty("payments")]
        public List<Payment> Payments { get; set; }

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
