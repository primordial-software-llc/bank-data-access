using Newtonsoft.Json;

namespace FinanceApi.RequestModels
{
    class CreatePublicTokenModel
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }
    }
}
