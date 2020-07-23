using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Tax
{
    public class TaxRateDetail
    {
        [JsonProperty("TaxRateRef")]
        public Reference TaxRateRef { get; set; }
    }
}
