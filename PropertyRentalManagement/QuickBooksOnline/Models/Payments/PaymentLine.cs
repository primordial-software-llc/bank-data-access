using System.Collections.Generic;
using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Payments
{
    public class PaymentLine
    {
        [JsonProperty("Amount")]
        public decimal Amount { get; set; }

        [JsonProperty("LinkedTxn")]
        public List<LinkedTransaction> LinkedTxn { get; set; }
    }
}
