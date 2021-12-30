using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class Item : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "Item";

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("SyncToken")]
        public string SyncToken { get; set; }

        [JsonProperty("Id")]
        public int? Id { get; set; }

        [JsonProperty("sparse")]
        public bool? Sparse { get; set; }
    }
}
