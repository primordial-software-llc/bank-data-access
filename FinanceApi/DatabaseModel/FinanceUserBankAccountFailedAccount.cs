using Newtonsoft.Json;

namespace FinanceApi.DatabaseModel
{
    public class FinanceUserBankAccountFailedAccount
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("errorDescription")]
        public string ErrorDescription { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }
    }
}
