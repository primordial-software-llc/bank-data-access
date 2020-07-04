using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class Invoice : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "Invoice";

        [JsonProperty("TxnDate")]
        public string TxnDate { get; set; }

        [JsonProperty("TotalAmt")]
        public decimal TotalAmount { get; set; }
    }
}
