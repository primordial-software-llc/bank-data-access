using System.Collections.Generic;
using Newtonsoft.Json;

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
        public List<Transaction> Transactions { get; set; }
    }
}
