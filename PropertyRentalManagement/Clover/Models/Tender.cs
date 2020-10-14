using Newtonsoft.Json;

namespace PropertyRentalManagement.Clover.Models
{
    public class Tender
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("labelKey")]
        public string LabelKey { get; set; }
    }
}
