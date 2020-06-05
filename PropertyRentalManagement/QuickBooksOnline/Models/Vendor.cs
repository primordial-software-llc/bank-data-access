using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class Vendor : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "Vendor";

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Id")]
        public string Id { get; set; }
    }
}
