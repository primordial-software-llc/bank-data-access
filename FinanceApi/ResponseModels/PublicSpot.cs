using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;

namespace FinanceApi.ResponseModels
{
    public class PublicSpot
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("section")]
        public Section Section { get; set; }

        [JsonProperty("right")]
        public string Right { get; set; }

        [JsonProperty("bottom")]
        public string Bottom { get; set; }

        [JsonProperty("reservedBy")]
        public string ReservedBy { get; set; }
    }
}
