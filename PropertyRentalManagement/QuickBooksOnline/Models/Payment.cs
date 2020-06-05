using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class Payment : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "Payment";

        [JsonProperty("CustomerRef")]
        public Reference CustomerRef { get; set; }

        [JsonProperty("TxnDate")]
        public string TxnDate { get; set; }

        [JsonProperty("TotalAmt")]
        public decimal TotalAmount { get; set; }
    }
}
