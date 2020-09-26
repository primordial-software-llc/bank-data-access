using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.PlaidModel
{
    public class TransactionsResponse
    {
        [JsonProperty("accounts")]
        public List<Account> Accounts { get; set; }

        [JsonProperty("item")]
        public Item Item { get; set; }

        [JsonProperty("total_transactions")]
        public int TotalTransactions { get; set; }

        [JsonProperty("transactions")]
        public List<JObject> Transactions { get; set; }
    }
}
