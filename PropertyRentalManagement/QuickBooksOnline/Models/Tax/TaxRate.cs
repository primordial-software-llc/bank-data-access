using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Tax
{
    public class TaxRate : IQuickBooksOnlineEntity
    {
        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("RateValue")]
        public decimal? RateValue { get; set; }

        [JsonIgnore]
        public string EntityName => "TaxRate";

        [JsonProperty("sparse")]
        public bool? Sparse { get; set; }
    }
}
