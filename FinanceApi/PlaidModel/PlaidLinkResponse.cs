using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.PlaidModel
{
    public class PlaidLinkResponse
	{
        //"institution": { "name": "Bank of America","institution_id": "ins_1" },
        [JsonProperty("institution")]
        public JObject Institution { get; set; }

        //{"id": null, "name": null, "type": null, "subtype": null, "mask": null }
        [JsonProperty("account")]
        public JObject Account { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("accounts")]
        public List<PlaidLinkAccount> Accounts { get; set; }

        [JsonProperty("public_token")]
        public string PublicToken { get; set; }
    }
}
