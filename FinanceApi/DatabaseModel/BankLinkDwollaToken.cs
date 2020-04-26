using Newtonsoft.Json;

namespace FinanceApi.DatabaseModel
{
    public class BankLinkDwollaToken
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("processorToken")]
        public string ProcessorToken { get; set; }
    }
}
