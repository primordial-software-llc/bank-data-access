using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Spot : IModel
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

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = Id } } };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-spots";
        }
    }
}
