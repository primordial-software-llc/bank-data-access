using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsDataAccess;
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

        [JsonProperty("restricted")]
        public bool? Restricted { get; set; }

        [JsonProperty("heightInFeet")]
        public decimal? HeightInFeet { get; set; }

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
