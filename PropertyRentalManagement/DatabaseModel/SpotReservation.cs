using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class SpotReservation : IModel
    {
        [JsonProperty("spotId")]
        public string SpotId { get; set; }

        [JsonProperty("rentalDate")]
        public string RentalDate { get; set; }

        [JsonProperty("quickBooksOnlineCustomerId")]
        public int? QuickBooksOnlineCustomerId { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue>
            {
                { "spotId", new AttributeValue { S = SpotId } },
                { "rentalDate", new AttributeValue { S = RentalDate } }
            };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-spot-reservation";
        }
    }
}
