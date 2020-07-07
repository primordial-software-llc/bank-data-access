using Newtonsoft.Json;

namespace FinanceApi.RequestModels
{
    class LinkAccessTokenModel
    {
        [JsonProperty("public_token")]
        public string PublicToken { get; set; }
    }
}
