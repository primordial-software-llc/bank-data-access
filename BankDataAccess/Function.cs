using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using BankDataAccess.DatabaseModel;
using BankDataAccess.PlaidModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace BankDataAccess
{
    public class Function
    {
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var client = new BankClient(PlaidConfiguration.DEV_URL, new Logger());
            var response = new APIGatewayProxyResponse();
            response.Headers = new Dictionary<string, string>();
            response.Headers.Add("access-control-allow-origin", "*");
            response.StatusCode = 200;
            JToken json;
            var authHeader = request.Headers["Authorization"];
            var payload = Base64Decode(authHeader.Split('.')[1]);
            var email = JObject.Parse(payload)["email"].Value<string>();
            Console.WriteLine("User accessed API's: " + email);
            var databaseClient = new DatabaseClient<BankDataAccessUser>(new AmazonDynamoDBClient());
            var updateSerializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
            var user = databaseClient.Get(new BankDataAccessUser { Email = email });
            try
            {
                if (string.Equals(request.HttpMethod, "DELETE", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(request.Path, "/bank-link", StringComparison.OrdinalIgnoreCase))
                {
                    var body = JObject.Parse(request.Body);
                    var link = user.BankLinks.First(x =>
                        string.Equals(x.ItemId, body["itemId"].Value<string>(), StringComparison.OrdinalIgnoreCase));
                    client.RemoveItem(link.AccessToken);
                    user.BankLinks.Remove(link);
                    var update = new BankDataAccessUser();
                    update.BankLinks = user.BankLinks;
                    var updateItemResponse = UpdateUser(email, JObject.FromObject(update, updateSerializer));
                    response.StatusCode = (int)updateItemResponse.HttpStatusCode;
                    json = new JObject();
                }
                else if (string.Equals(request.HttpMethod, "GET") &&
                    string.Equals(request.Path, "/bank-link"))
                {
                    var itemJson = new JArray();
                    var institutionsJson = new JArray();
                    var institutions = new HashSet<string>();
                    foreach (var bankLink in user.BankLinks ?? new List<BankLink>())
                    {
                        var item = client.GetItem(bankLink.AccessToken)["item"];
                        institutions.Add(item["institution_id"].Value<string>());
                        itemJson.Add(item);
                    }
                    foreach (var institution in institutions)
                    {
                        institutionsJson.Add(client.GetInstitution(institution)["institution"]);
                    }
                    foreach (var item in itemJson)
                    {
                        var institutionId = item["institution_id"].Value<string>();
                        item["institution"] = institutionsJson.First(x =>
                            string.Equals(x["institution_id"].Value<string>(), institutionId, StringComparison.OrdinalIgnoreCase));
                    }
                    json = itemJson;
                }
                else if (string.Equals(request.HttpMethod, "POST") &&
                    string.Equals(request.Path, "/create-public-token"))
                {
                    var body = JObject.Parse(request.Body);
                    var link = (user.BankLinks ?? new List<BankLink>())
                        .First(x => string.Equals(x.ItemId, body["itemId"].Value<string>(), StringComparison.OrdinalIgnoreCase));
                    json = client.CreatePublicToken(link.AccessToken);
                }
                else if (string.Equals(request.HttpMethod, "POST") &&
                    string.Equals(request.Path, "/link-access-token"))
                {
                    var body = JObject.Parse(request.Body);
                    var accessTokenJson = client.GetAccessToken(body["publicToken"].Value<string>());
                    var update = new BankDataAccessUser();
                    // I don't like having a big model and using the c# model to update it.
                    // Non nullable types could overwrite data.
                    // c# models should be used for read only purposes to not alter data unintentionally.
                    update.BankLinks = user.BankLinks ?? new List<BankLink>();
                    update.BankLinks.Add(new BankLink
                    {
                        AccessToken = accessTokenJson["access_token"].Value<string>(),
                        ItemId = accessTokenJson["item_id"].Value<string>()
                    });
                    var updateItemResponse = UpdateUser(email, JObject.FromObject(update, updateSerializer));
                    response.StatusCode = (int)updateItemResponse.HttpStatusCode;
                    json = new JObject();
                }
                else if (string.Equals(request.HttpMethod, "PATCH") &&
                    string.Equals(request.Path, "/budget"))
                {
                    var jsonPatch = JObject.Parse(request.Body);
                    if (jsonPatch["licenseAgreement"] != null &&
                        jsonPatch["licenseAgreement"]["agreedToLicense"] != null &&
                        jsonPatch["licenseAgreement"]["agreedToLicense"].Value<bool>() &&
                        !(user.LicenseAgreement?.AgreedToLicense).GetValueOrDefault())
                    {
                        jsonPatch["licenseAgreement"]["ipAddress"] = request.RequestContext.Identity.SourceIp;
                        jsonPatch["licenseAgreement"]["agreementDateUtc"] = DateTime.UtcNow.ToString("O");
                    }
                    else
                    {
                        jsonPatch["licenseAgreement"] = null;
                    }
                    var updateItemResponse = UpdateUser(email, jsonPatch);
                    json = JObject.Parse(Document.FromAttributeMap(updateItemResponse.Attributes).ToJson());
                    response.StatusCode = (int) updateItemResponse.HttpStatusCode;
                }
                else if (string.Equals(request.HttpMethod, "GET") &&
                         string.Equals(request.Path, "/budget"))
                {
                    user.BankLinks = null; // Don't send this data to the client. It should get moved to another table that's encrypted.
                    json = JObject.Parse(JsonConvert.SerializeObject(user));
                }
                else if (string.Equals(request.HttpMethod, "GET") &&
                         string.Equals(request.Path, "/accountBalance"))
                {
                    var accounts = new List<AccountBalance>();
                    var institutionsJson = new JArray();
                    var institutions = new HashSet<string>();
                    foreach (var bankLink in user.BankLinks ?? new List<BankLink>())
                    {
                        var accountBalance = client.GetAccountBalance(bankLink.AccessToken);
                        institutions.Add(accountBalance.Item["institution_id"].Value<string>());
                        accounts.Add(accountBalance);
                    }
                    foreach (var institution in institutions)
                    {
                        institutionsJson.Add(client.GetInstitution(institution)["institution"]);
                    }
                    foreach (var account in accounts)
                    {
                        var institutionId = account.Item["institution_id"].Value<string>();
                        account.Item["institution"] = institutionsJson.First(x =>
                            string.Equals(x["institution_id"].Value<string>(), institutionId, StringComparison.OrdinalIgnoreCase));
                    }
                    json = new JObject {{"allAccounts", JArray.FromObject(accounts)}};
                }
                else
                {
                    response.StatusCode = 404;
                    json = new JObject();
                }
            }
            catch (Exception exception)
            {
                response.StatusCode = 500;
                json = new JObject {{"error", exception.ToString()}};
            }
            response.Body = json.ToString();
            return response;
        }

        private UpdateItemResponse UpdateUser(string email, JObject update)
        {
            var dbClient = new AmazonDynamoDBClient();
            return dbClient.UpdateItemAsync(
                new BankDataAccessUser().GetTable(),
                BankDataAccessUser.GetKey(email),
                Document.FromJson(update.ToString()).ToAttributeUpdateMap(false),
                ReturnValue.ALL_NEW
            ).Result;
        }

        private string Base64Decode(string base64Encoded)
        {
            base64Encoded = base64Encoded.PadRight(base64Encoded.Length + (base64Encoded.Length * 3) % 4, '=');
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));
        }
    }
}
