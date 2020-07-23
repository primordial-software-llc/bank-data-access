using Accounting;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class JournalEntry : IJournalEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("accountingId")]
        public int? AccountingId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("account")]
        public int? Account { get; set; }

        [JsonProperty("product")]
        public int? Product { get; set; }

        [JsonProperty("taxCode")]
        public int? TaxCode { get; set; }

        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("memo")]
        public string Memo { get; set; }

        [JsonProperty("batch")]
        public string Batch { get; set; }
    }
}
