using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline.Models.Invoices;
using PropertyRentalManagement.QuickBooksOnline.Models.Payments;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ReceiptSaveResult
    {
        [JsonProperty("receipt")]
        public Receipt Receipt { get; set; }
        
        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }

        [JsonProperty("paymentAppliedToInvoice")]
        public Payment PaymentAppliedToInvoice { get; set; }

        [JsonProperty("unappliedPayment")]
        public Payment UnappliedPayment { get; set; }
    }
}
