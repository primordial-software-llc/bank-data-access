using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Receipt
    {
        [JsonProperty("rentalDate")]
        public string RentalDate { get; set; }

        [JsonProperty("transactionDate")]
        public string TransactionDate { get; set; }

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
    }
}
