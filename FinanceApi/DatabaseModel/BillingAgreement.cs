using Newtonsoft.Json;

namespace FinanceApi.DatabaseModel
{
    public class BillingAgreement
    {
        [JsonProperty("agreedToBillingTerms")]
        public bool AgreedToLicense { get; set; }
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        [JsonProperty("agreementDateUtc")]
        public string Date { get; set; }
    }
}
