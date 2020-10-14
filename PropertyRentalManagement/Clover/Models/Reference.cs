using Newtonsoft.Json;

namespace PropertyRentalManagement.Clover.Models
{
    public class Reference
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
