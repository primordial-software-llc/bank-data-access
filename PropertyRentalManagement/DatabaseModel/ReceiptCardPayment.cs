using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class ReceiptCardPayment
    {
        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("expirationMonth")]
        public string ExpirationMonth { get; set; }

        [JsonProperty("expirationYear")]
        public string ExpirationYear { get; set; }

        [JsonProperty("cvv")]
        public string Cvv { get; set; }
    }
}
