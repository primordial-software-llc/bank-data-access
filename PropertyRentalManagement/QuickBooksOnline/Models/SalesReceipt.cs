using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class SalesReceipt : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "SalesReceipt";

        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("CustomerRef")]
        public Reference CustomerRef { get; set; }

        [JsonProperty("TxnDate")]
        public string TxnDate { get; set; }

        [JsonProperty("TotalAmt")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("MetaData")]
        public MetaData MetaData { get; set; }

        [JsonProperty("sparse")]
        public bool? Sparse { get; set; }
    }
}
