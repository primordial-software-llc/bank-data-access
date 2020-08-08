using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Section
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
