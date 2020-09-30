using System.Collections.Generic;
using Newtonsoft.Json;

namespace FinanceApi.PlaidModel
{
    public class Transaction
	{
        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("authorized_date")]
        public string AuthorizedDate { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("category")]
        public List<string> Category { get; set; }

        [JsonProperty("merchant_name")]
        public string MerchantName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("payment_channel")]
        public string PaymentChannel { get; set; }

        [JsonProperty("pending")]
        public bool? Pending { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }
    }
}
