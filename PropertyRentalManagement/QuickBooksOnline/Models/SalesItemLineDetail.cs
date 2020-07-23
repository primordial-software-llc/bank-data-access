using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class SalesItemLineDetail
    {
        [JsonProperty("TaxCodeRef")]
        public Reference TaxCodeRef { get; set; }

        [JsonProperty("Qty")]
        public decimal? Quantity { get; set; }

        [JsonProperty("UnitPrice")]
        public decimal? UnitPrice { get; set; }

        [JsonProperty("ItemRef")]
        public Reference ItemRef { get; set; }
    }
}
