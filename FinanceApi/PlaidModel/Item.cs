using Newtonsoft.Json;

namespace FinanceApi.PlaidModel
{
    public class Item
    {
        [JsonProperty("institution_id")]
        public string InstitutionId { get; set; }
    }
}
