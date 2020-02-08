using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json.Linq;

namespace FinanceApi
{
    class UserService
    {
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
                FinanceUser.GetKey(email),
                Document.FromJson(update.ToString()).ToAttributeUpdateMap(false),
                ReturnValue.ALL_NEW
            ).Result;
        }
    }
}
