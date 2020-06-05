using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class Customer : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "Customer";

        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("MetaData")]
        public MetaData MetaData { get; set; }
    }
}
