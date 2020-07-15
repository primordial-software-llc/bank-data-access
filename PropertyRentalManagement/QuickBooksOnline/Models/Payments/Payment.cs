using System.Collections.Generic;
using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Payments
{
    public class Payment : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "Payment";

        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("CustomerRef")]
        public Reference CustomerRef { get; set; }

        [JsonProperty("TxnDate")]
        public string TxnDate { get; set; }

        [JsonProperty("TotalAmt")]
        public decimal? TotalAmount { get; set; }

        [JsonProperty("UnappliedAmt")]
        public decimal? UnappliedAmt { get; set; }

        [JsonProperty("PrivateNote")]
        public string PrivateNote { get; set; }

        [JsonProperty("Line")]
        public List<PaymentLine> Line { get; set; }

        [JsonProperty("MetaData")]
        public MetaData MetaData { get; set; }

        [JsonProperty("sparse")]
        public bool? Sparse { get; set; }
    }
}
