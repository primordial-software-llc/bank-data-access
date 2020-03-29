using Newtonsoft.Json;

namespace FinanceApi.RequestModels
{
    class LinkAccessTokenModel
    {
        [JsonProperty("publicToken")]
        public string PublicToken { get; set; }
    }
}
