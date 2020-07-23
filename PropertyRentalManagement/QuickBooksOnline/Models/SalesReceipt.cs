using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;

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

        [JsonProperty("Line")]
        public List<SalesLine> Line { get; set; }

        [JsonProperty("TxnTaxDetail")]
        public TxnTaxDetail TxnTaxDetail { get; set; }

        [JsonProperty("TotalAmt")]
        public decimal? TotalAmount { get; set; }

        [JsonProperty("PrivateNote")]
        public string PrivateNote { get; set; }

        [JsonProperty("MetaData")]
        public MetaData MetaData { get; set; }

        [JsonProperty("sparse")]
        public bool? Sparse { get; set; }
    }
}
