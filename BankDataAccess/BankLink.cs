using Newtonsoft.Json;

namespace BankDataAccess
{
    public class BankLink
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("itemId")]
        public string ItemId { get; set; }
    }
}
