using FinanceApi.PlaidModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.ResponseModels
{
    public class InstitutionAccountTransaction
    {
        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("transactionDetail")]
        public JObject TransactionDetail { get; set; }

        [JsonProperty("account")]
        public Account Account { get; set; }
    }
}
