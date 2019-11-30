using Newtonsoft.Json;

namespace FinanceApi.DatabaseModel
{
    public class LicenseAgreement
    {
        [JsonProperty("agreedToLicense")]
        public bool AgreedToLicense { get; set; }
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        [JsonProperty("agreementDateUtc")]
        public string Date { get; set; }
    }
}
