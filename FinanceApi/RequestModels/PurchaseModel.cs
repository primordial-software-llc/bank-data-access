using Newtonsoft.Json;

namespace FinanceApi.RequestModels
{
    public class PurchaseModel
    {
        [JsonProperty("agreedToBillingTerms")]
        public bool AgreedToBillingTerms { get; set; }

        [JsonProperty("cardCvc")]
        public string CardCvc { get; set; }

        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("cardExpirationMonth")]
        public long CardExpirationMonth { get; set; }

        [JsonProperty("cardExpirationYear")]
        public long CardExpirationYear { get; set; }
    }
}
