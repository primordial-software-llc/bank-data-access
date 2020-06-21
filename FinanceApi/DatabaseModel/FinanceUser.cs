using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace FinanceApi.DatabaseModel
{
    public class FinanceUser : IModel
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("licenseAgreement")]
        public LicenseAgreement LicenseAgreement { get; set; }

        [JsonProperty("billingAgreement")]
        public BillingAgreement BillingAgreement { get; set; }

        [JsonProperty("bankLinks")]
        public List<BankLink> BankLinks { get; set; }

        [JsonProperty("401k-contribution-for-year")]
        public string ContributionTo401KForYear { get; set; }

        [JsonProperty("401k-contribution-per-pay-check")]
        public string ContributionTo401KPerPayCheck { get; set; }

        [JsonProperty("assets")]
        public List<JObject> Assets { get; set; }

        [JsonProperty("balances")]
        public List<JObject> Balances { get; set; }

        [JsonProperty("biweekly")]
        public List<JObject> Biweekly { get; set; }

        [JsonProperty("monthlyRecurringExpenses")]
        public List<JObject> MonthlyRecurringExpenses { get; set; }

        [JsonProperty("pending")]
        public List<JObject> Pending { get; set; }

        [JsonProperty("propertyPlantAndEquipment")]
        public List<JObject> PropertyPlantAndEquipment { get; set; }

        [JsonProperty("weeklyRecurringExpenses")]
        public List<JObject> WeeklyRecurringExpenses { get; set; }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return new Dictionary<string, AttributeValue> { { "email", new AttributeValue { S = Email } } };

        }

        public string GetTable()
        {
            return "Finance-Users";
        }
    }
}
