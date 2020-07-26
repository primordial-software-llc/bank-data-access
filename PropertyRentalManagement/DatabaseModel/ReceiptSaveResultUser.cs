
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class ReceiptSaveResultUser
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
