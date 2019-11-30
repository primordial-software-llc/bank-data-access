using Newtonsoft.Json;

namespace FinanceApi
{
    public class BankLink
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("itemId")]
        public string ItemId { get; set; }
    }
}
