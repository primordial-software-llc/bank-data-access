using Newtonsoft.Json;

namespace FinanceApi.CloverModel
{
    public class TokenizeCardRequest
    {
        [JsonProperty("card")]
        public Card Card { get; set; }
    }
}
