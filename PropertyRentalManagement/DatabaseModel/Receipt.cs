using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Receipt
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("rentalDate")]
        public string RentalDate { get; set; }

        [JsonProperty("customer")]
        public Reference Customer { get; set; }
        
        [JsonProperty("amountOfAccount")]
        public decimal? AmountOfAccount { get; set; }
        
        [JsonProperty("rentalAmount")]
        public decimal? RentalAmount { get; set; }
        
        [JsonProperty("thisPayment")]
        public decimal? ThisPayment { get; set; }
        
        [JsonProperty("memo")]
        public string Memo { get; set; }

        [JsonProperty("spots")]
        public List<Spot> Spots { get; set; }

        [JsonProperty("makeCardPayment")]
        public bool? MakeCardPayment { get; set; }

        [JsonProperty("cardPayment")]
        public ReceiptCardPayment CardPayment { get; set; }

        [JsonProperty("isCardPresent")]
        public bool? IsCardPresent { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

    }
}
