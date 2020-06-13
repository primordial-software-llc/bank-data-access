using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Invoices
{
    public class TxnTaxDetail
    {
        [JsonProperty("TxnTaxCodeRef")]
        public Reference TxnTaxCodeRef { get; set; }
    }
}
