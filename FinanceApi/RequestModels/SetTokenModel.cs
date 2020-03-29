using Newtonsoft.Json;

namespace FinanceApi.RequestModels
{
    class SetTokenModel
    {
        [JsonProperty("idToken")]
        public string IdToken { get; set; }
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
