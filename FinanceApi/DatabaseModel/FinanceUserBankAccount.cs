using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.DatabaseModel
{
    public class FinanceUserBankAccount : IModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("allAccounts")]
        public JArray AllAccounts { get; set; }

        [JsonProperty("failedAccounts")]
        public JArray FailedAccounts { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { { "email", new AttributeValue { S = Email } } };
        }

        public string GetTable()
        {
            return "Finance-Users-Bank-Account";
        }
    }
}
