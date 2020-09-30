using FinanceApi.PlaidModel;
using Newtonsoft.Json;

namespace FinanceApi.ResponseModels
{
    public class InstitutionAccountTransaction
    {
        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("transactionDetail")]
        public Transaction TransactionDetail { get; set; }

        [JsonProperty("account")]
        public Account Account { get; set; }
    }
}
