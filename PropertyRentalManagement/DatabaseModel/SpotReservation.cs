using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class SpotReservation : IModel
    {
        [JsonProperty("rentalDate")]
        public string RentalDate { get; set; }

        [JsonProperty("spotId")]
        public string SpotId { get; set; }

        [JsonProperty("quickBooksOnlineId")]
        public int? QuickBooksOnlineId { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue>
            {
                { "rentalDate", new AttributeValue { S = RentalDate } },
                { "spotId", new AttributeValue { S = SpotId } }
            };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-spot-reservation";
        }

    }
}
