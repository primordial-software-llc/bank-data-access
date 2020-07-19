using System.Collections.Generic;
using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class Purchase : IQuickBooksOnlineEntity
    {
        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("TotalAmt")]
        public decimal? TotalAmount { get; set; }

        [JsonProperty("AccountRef")]
        public Reference AccountRef { get; set; }

        [JsonProperty("PaymentType")]
        public string PaymentType { get; set; }

        [JsonProperty("TxnDate")]
        public string TxnDate { get; set; }

        [JsonProperty("Line")]
        public List<PurchaseLine> Line { get; set; }

        [JsonProperty("PrivateNote")]
        public string PrivateNote { get; set; }

        [JsonProperty("EntityRef")]
        public Reference EntityRef { get; set; }

        [JsonProperty("DocNumber")]
        public string DocNumber { get; set; }

        [JsonIgnore]
        public string EntityName => "Purchase";

        [JsonProperty("sparse")]
        public bool? Sparse { get; set; }
    }
}
