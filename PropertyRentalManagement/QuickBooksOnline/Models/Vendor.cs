using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class Vendor : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "Vendor";

        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("sparse")]
        public bool? Sparse { get; set; }
    }
}
