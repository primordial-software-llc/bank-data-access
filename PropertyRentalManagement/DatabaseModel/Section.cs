using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class Section : IModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("spots")]
        public List<Spot> Spots { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = Id } } };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-sections";
        }
    }
}
