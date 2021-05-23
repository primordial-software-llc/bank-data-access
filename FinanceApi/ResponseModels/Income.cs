using Newtonsoft.Json;

namespace FinanceApi.ResponseModels
{
    public class Income
    {
        [JsonProperty("accountingId")]
        public string AccountingId { get; set; }

        [JsonProperty("recordType")]
        public string RecordType { get; set; }

        [JsonProperty("customer")]
        public string Customer { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }
    }
}
