using Newtonsoft.Json;

namespace FinanceApi.CloverModel
{
    public class Card
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("exp_month")]
        public string ExpirationMonth { get; set; }

        [JsonProperty("exp_year")]
        public string ExpirationYear { get; set; }

        [JsonProperty("cvv")]
        public string Cvv { get; set; }
    }
}
