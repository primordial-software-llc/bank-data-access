using Newtonsoft.Json;

namespace FinanceApi.RequestModels
{
    class DeleteBankLinkModel
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }
    }
}
