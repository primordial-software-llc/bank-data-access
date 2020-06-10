using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class PhoneNumber
    {
        [JsonProperty("FreeFormNumber")]
        public string FreeFormNumber { get; set; }
    }
}
