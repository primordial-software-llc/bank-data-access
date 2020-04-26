using Newtonsoft.Json;

namespace FinanceApi.PlaidModel
{
    public class AccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("item_id")]
        public string ItemId { get; set; }
    }
}
