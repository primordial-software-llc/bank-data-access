using System.Collections.Generic;
using Newtonsoft.Json;

namespace FinanceApi.DatabaseModel
{
    public class BankLink
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("dwollaProcessorTokens")]
        public List<BankLinkDwollaToken> DwollaProcessorTokens { get; set; }
    }
}
