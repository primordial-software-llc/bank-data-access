using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BankDataAccess
{
    public class BankDataAccessUser : IModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("401k-contribution-for-year")]
        public string ContributionTo401KForYear { get; set; }
        [JsonProperty("401k-contribution-per-pay-check")]
        public string ContributionTo401KPerPayCheck { get; set; }
        [JsonProperty("accessTokens")]
        public List<string> AccessTokens { get; set; }
        [JsonProperty("assets")]
        public List<JObject> Assets { get; set; }
        [JsonProperty("balances")]
        public List<JObject> Balances { get; set; }
        [JsonProperty("biWeeklyIncome")]
        public JObject BiWeeklyIncome { get; set; }
        [JsonProperty("monthlyRecurringExpenses")]
        public List<JObject> MonthlyRecurringExpenses { get; set; }
        [JsonProperty("paymentSources")]
        public List<string> PaymentSources { get; set; }
        [JsonProperty("pending")]
        public List<JObject> Pending { get; set; }
        [JsonProperty("propertyPlantAndEquipment")]
        public List<JObject> PropertyPlantAndEquipment { get; set; }
        [JsonProperty("weeklyRecurringExpenses")]
        public List<JObject> WeeklyRecurringExpenses { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { {"email", new AttributeValue {S = Email}} };
        }

        public string GetTable()
        {
            return "bank-data-access-users";
        }
    }
}
