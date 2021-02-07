using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.PlaidModel
{
    class CreateLinkToken
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("secret")]
        public string Secret { get; set; }

        [JsonProperty("client_name")]
        public string ClientName { get; set; }

        [JsonProperty("user")]
        public JObject User { get; set; }

        [JsonProperty("country_codes")]
        public List<string> CountryCodes { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("products")]
        public List<string> Products { get; set; }
    }
}
