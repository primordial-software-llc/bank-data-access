using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class PurchaseLine
    {
        [JsonProperty("Amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("DetailType")]
        public string DetailType { get; set; }

        [JsonProperty("AccountBasedExpenseLineDetail")]
        public AccountBasedExpenseLineDetail AccountBasedExpenseLineDetail { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }
    }
}
