using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;

namespace PropertyRentalManagement.DatabaseModel
{
    public class QuickBooksOnlineConnection : IModel
    {
        [JsonProperty("realm-id")]
        public string RealmId { get; set; }

        [JsonProperty("client-id")]
        public string ClientId { get; set; }

        [JsonProperty("client-secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("refresh-token")]
        public string RefreshToken { get; set; }

        [JsonProperty("access-token")]
        public string AccessToken { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { { "realm-id", new AttributeValue { S = RealmId } } };
        }

        public string GetTable()
        {
            return "lakeland-mi-pueblo-quickbooks-online-connections";
        }
    }
}
