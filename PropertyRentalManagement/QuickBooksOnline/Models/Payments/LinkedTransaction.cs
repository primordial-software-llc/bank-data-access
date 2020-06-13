using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Payments
{
    public class LinkedTransaction
    {
        [JsonProperty("TxnId")]
        public string TxnId { get; set; }

        [JsonProperty("TxnType")]
        public string TxnType { get; set; }
    }
}
