using Newtonsoft.Json;

namespace FinanceApi.RequestModels
{
    class SignupModel
    {
        [JsonProperty("agreedToLicense")]
        public bool AgreedToLicense { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
