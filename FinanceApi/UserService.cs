using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json.Linq;

namespace FinanceApi
{
    class UserService
    {
        public void CreateUser(string email, bool agreedToLicense, string ip)
        {
            var user = new FinanceUser
            {
                Email = email,
                Biweekly = new List<JObject>(),
                MonthlyRecurringExpenses = new List<JObject>(),
                WeeklyRecurringExpenses = new List<JObject>(),
                LicenseAgreement = new LicenseAgreement
                {
                    AgreedToLicense = agreedToLicense,
                    Date = DateTime.UtcNow.ToString("O"),
                    IpAddress = ip
                }
            };
            var dbClient = new DatabaseClient<FinanceUser>(new AmazonDynamoDBClient());
            dbClient.Create(user);
        }

        // I NEED TO WHITE LIST THE PROPERTIES FOR UPDATE INSTEAD OF BLACKLIST, but I have far greater risks at the moment than exposing an authenticated users data to themselves.
        public UpdateItemResponse UpdateUser(string email, JObject update, bool allowBillingAgreementUpdate = false)
        {
            if (update["licenseAgreement"] != null)
            {
                update.Remove("licenseAgreement");
            }
            if (!allowBillingAgreementUpdate && update["billingAgreement"] != null)
            {
                update.Remove("billingAgreement");
            }
            var dbClient = new AmazonDynamoDBClient();
            return dbClient.UpdateItemAsync(
                new FinanceUser().GetTable(),
                new FinanceUser { Email = email }.GetKey(),
                Document.FromJson(update.ToString()).ToAttributeUpdateMap(false),
                ReturnValue.ALL_NEW
            ).Result;
        }
    }
}
