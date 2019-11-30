using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FinanceApi.DatabaseModel;
using FinanceApi.PlaidModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace FinanceApi
{
    public class Function
    {
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var client = new BankAccessClient(Configuration.DEV_URL, new Logger());
            var clientDomain = "https://www.primordial-software.com";
            var response = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    {"access-control-allow-origin", clientDomain},
                    {"Access-Control-Allow-Credentials", "true" }
                },
                StatusCode = 200
            };
            string token = GetCookie(request, "idToken");
            bool isAuthenticated = false;
            if (!request.Path.StartsWith("/unauthenticated/", StringComparison.OrdinalIgnoreCase))
            {
                isAuthenticated = AwsCognitoJwtTokenValidator.IsValid(token, Configuration.FINANCE_API_COGNITO_USER_POOL_ID);
                if (!isAuthenticated)
                {
                    response.StatusCode = 401;
                    response.Body = new JObject { { "error", "The incoming token is invalid" } }.ToString();
                    return response;
                }
            }

            JToken json;
            FinanceUser user = null;
            string email = string.Empty;
            if (isAuthenticated)
            {
                var payload = Base64Decode(token.Split('.')[1]);
                email = JObject.Parse(payload)["email"].Value<string>();
                Console.WriteLine("User accessed API's: " + email);
                var databaseClient = new DatabaseClient<FinanceUser>(new AmazonDynamoDBClient());
                user = databaseClient.Get(new FinanceUser { Email = email });
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    user = new FinanceUser
                    {
                        Email = email,
                        BiWeeklyIncome = new JObject { { "date", "2015-12-25T00:00:00Z" }, { "amount", 0 } },
                        MonthlyRecurringExpenses = new List<JObject>(),
                        WeeklyRecurringExpenses = new List<JObject>()
                    };
                    var update = JObject.FromObject(user, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
                    var dbClient = new AmazonDynamoDBClient();
                    var createResponse = dbClient.PutItemAsync(
                        new FinanceUser().GetTable(),
                        Document.FromJson(update.ToString()).ToAttributeMap()
                    ).Result;
                }
            }

            try
            {
                if (string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(request.Path, "/unauthenticated/setToken", StringComparison.OrdinalIgnoreCase))
                {
                    var body = JObject.Parse(request.Body);
                    response.MultiValueHeaders = new Dictionary<string, IList<string>>();
                    response.MultiValueHeaders.Add("Set-Cookie", new List<string>
                    {
                        $"idToken={body["idToken"]};Path=/;Secure;HttpOnly",
                        $"refreshToken={body["refreshToken"]};Path=/;Secure;HttpOnly"
                    });
                    json = new JObject();
                }
                else if (string.Equals(request.HttpMethod, "DELETE", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(request.Path, "/bank-link", StringComparison.OrdinalIgnoreCase))
                {
                    var body = JObject.Parse(request.Body);
                    var link = user.BankLinks.First(x =>
                        string.Equals(x.ItemId, body["itemId"].Value<string>(), StringComparison.OrdinalIgnoreCase));
                    client.RemoveItem(link.AccessToken);
                    user.BankLinks.Remove(link);
                    var update = new JObject {{"bankLinks", JToken.FromObject(user.BankLinks)}};
                    var updateItemResponse = UpdateUser(email, update);
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
                    var updatedBankLinks = user.BankLinks ?? new List<BankLink>();
                    updatedBankLinks.Add(new BankLink
                    {
                        AccessToken = accessTokenJson["access_token"].Value<string>(),
                        ItemId = accessTokenJson["item_id"].Value<string>()
                    });
                    var update = new JObject {{"bankLinks", JToken.FromObject(updatedBankLinks)}};
                    var updateItemResponse = UpdateUser(email, update);
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
                    else if (jsonPatch["licenseAgreement"] != null)
                    {
                        jsonPatch.Remove("licenseAgreement");
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
                    var accounts = new ConcurrentBag<AccountBalance>();
                    var institutionsDictionary = new ConcurrentDictionary<string, string>();
                    user.BankLinks = user.BankLinks ?? new List<BankLink>();
                    Parallel.ForEach(user.BankLinks, (bankLink) =>
                    {
                        var accountBalance = client.GetAccountBalance(bankLink.AccessToken);
                        institutionsDictionary.TryAdd(
                            accountBalance.Item["institution_id"].Value<string>(),
                            accountBalance.Item["institution_id"].Value<string>());
                        accounts.Add(accountBalance);
                    });
                    var institutionDetails = new ConcurrentBag<JObject>();
                    Parallel.ForEach(institutionsDictionary.Keys, institution =>
                    {
                        institutionDetails.Add(client.GetInstitution(institution));
                    });
                    var institutionsJson = new JArray();
                    foreach (var institutionDetail in institutionDetails)
                    {
                        institutionsJson.Add(institutionDetail["institution"]);
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

        private string GetCookie(APIGatewayProxyRequest request, string cookieName)
        {
            var cookieHeader = request.MultiValueHeaders.Keys.FirstOrDefault(x => string.Equals(x, "cookie", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                return string.Empty;
            }
            var cookieValues = request.MultiValueHeaders[cookieHeader];
            foreach (var cookieValue in cookieValues)
            {
                var idTokenCookie = cookieValue.Split(';')
                    .Select(x => x.Trim())
                    .FirstOrDefault(x => string.Equals(x.Split("=")[0], cookieName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(idTokenCookie) && idTokenCookie.Split("=").Length > 0)
                {
                    return idTokenCookie.Split("=")[1];
                }
            }

            return string.Empty;
        }
        private UpdateItemResponse UpdateUser(string email, JObject update)
        {
            var dbClient = new AmazonDynamoDBClient();
            return dbClient.UpdateItemAsync(
                new FinanceUser().GetTable(),
                FinanceUser.GetKey(email),
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
