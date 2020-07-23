using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Tax
{
    public class TaxCode : IQuickBooksOnlineEntity
    {
        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("SalesTaxRateList")]
        public SalesTaxRateList SalesTaxRateList { get; set; }

        [JsonIgnore]
        public string EntityName => "TaxCode";

        [JsonProperty("sparse")]
        public bool? Sparse { get; set; }
    }
}
