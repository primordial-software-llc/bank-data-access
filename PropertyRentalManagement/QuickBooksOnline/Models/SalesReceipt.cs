using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class SalesReceipt : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "SalesReceipt";

        [JsonProperty("CustomerRef")]
        public Reference CustomerRef { get; set; }

        [JsonProperty("TxnDate")]
        public string TxnDate { get; set; }

        [JsonProperty("TotalAmt")]
        public decimal TotalAmount { get; set; }
    }
}
