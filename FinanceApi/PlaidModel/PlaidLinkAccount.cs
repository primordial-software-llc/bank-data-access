using Newtonsoft.Json;

namespace FinanceApi.PlaidModel
{
    public class PlaidLinkAccount
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mask")]
        public string Mask { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("subtype")]
        public string Subtype { get; set; }
    }
}
