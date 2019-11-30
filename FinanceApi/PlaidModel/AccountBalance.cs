using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.PlaidModel
{
    public class AccountBalance
    {
        [JsonProperty("accounts")]
        public JArray Accounts { get; set; }
        [JsonProperty("item")]
        public JObject Item { get; set; }
    }
}
