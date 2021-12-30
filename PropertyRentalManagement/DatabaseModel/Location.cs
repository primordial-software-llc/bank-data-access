using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsDataAccess;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Location : IModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rentProductId")]
        public int RentProductId { get; set; }

        [JsonProperty("salesRentalTaxRateId")]
        public int SalesRentalTaxRateId { get; set; }
        
        [JsonProperty("salesTaxRateId")]
        public int SalesTaxRateId { get; set; }

        [JsonProperty("invoiceItems")]
        public List<string> InvoiceItems { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = Id } } };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-location";
        }
    }
}
