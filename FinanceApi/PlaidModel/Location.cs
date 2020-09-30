using Newtonsoft.Json;

namespace FinanceApi.PlaidModel
{
    public class Location
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        
        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("lat")]
        public string Latitude { get; set; }

        [JsonProperty("lon")]
        public string Longitude { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("store_number")]
        public string StoreNumber { get; set; }
    }
}
