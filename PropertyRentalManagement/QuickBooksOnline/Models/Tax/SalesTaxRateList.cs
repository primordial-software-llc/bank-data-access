using System.Collections.Generic;
using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Tax
{
    public class SalesTaxRateList
    {
        [JsonProperty("TaxRateDetail")]
        public List<TaxRateDetail> TaxRateDetail { get; set; }
    }
}
