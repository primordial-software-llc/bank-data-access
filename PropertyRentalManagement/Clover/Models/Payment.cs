using Newtonsoft.Json;

namespace PropertyRentalManagement.Clover.Models
{
    public class Payment
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("tipAmount")]
        public decimal TipAmount { get; set; }

        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("cashbackAmount")]
        public decimal CashbackAmount { get; set; }

        [JsonProperty("tender")]
        public Reference Tender { get; set; }
    }
}
