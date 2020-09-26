using Newtonsoft.Json;

namespace FinanceApi.PlaidModel
{
    public class Account
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("mask")]
        public string Mask { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subtype")]
        public string Subtype { get; set; }
    }
}
