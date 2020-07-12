using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class Transfer : IQuickBooksOnlineEntity
    {
        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("TxnDate")]
        public string TxnDate { get; set; }

        [JsonProperty("Amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("ToAccountRef")]
        public Reference ToAccountRef { get; set; }

        [JsonProperty("FromAccountRef")]
        public Reference FromAccountRef { get; set; }

        [JsonProperty("PrivateNote")]
        public string PrivateNote { get; set; }

        [JsonIgnore]
        public string EntityName => "Transfer";
    }
}
