﻿using System.Collections.Generic;
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
        [JsonProperty("licenseAgreement")]
        public LicenseAgreement LicenseAgreement { get; set; }
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

        public static Dictionary<string, AttributeValue> GetKey(string email)
        {
            return new Dictionary<string, AttributeValue> { { "email", new AttributeValue { S = email } } };
        }

        public Dictionary<string, AttributeValue> GetKey()
        {
            return GetKey(Email);
        }

        public string GetTable()
        {
            return "Finance-Users";
        }
    }
}