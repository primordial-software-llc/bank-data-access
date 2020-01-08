using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using FinanceApi.DatabaseModel;
using FinanceApi.PlaidModel;
using FinanceApi.Routes;
using FinanceApi.Routes.Unauthenticated;
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
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());
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
            if (!string.IsNullOrWhiteSpace(token))
            {
                var payload = Base64Decode(token.Split('.')[1]);
                email = JObject.Parse(payload)["email"].Value<string>();
            }

            if (isAuthenticated)
            {
                try
                {
                    Console.WriteLine("Authenticated user accessed API's: " + email);
                    var databaseClient = new DatabaseClient<FinanceUser>(new AmazonDynamoDBClient());
                    user = databaseClient.Get(new FinanceUser { Email = email });
                    if (string.IsNullOrWhiteSpace(user.Email))
                    {
                        throw new Exception($"User not found {email}. Use the correct signup form at https://www.primordial-software.com/pages/login-signup.html");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    response.StatusCode = 500;
                    json = new JObject { { "error", exception.ToString() } };
                    response.Body = json.ToString();
                    return response;
                }
            }
            try
            {
                // You can move the sign-in here just for consistency. It's kind of silly to have a setToken method. You might as well make it meaningful.
                if (string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(request.Path, "/unauthenticated/setToken", StringComparison.OrdinalIgnoreCase))
                {
                    return new SetToken().Run(request, response);
                }
                else if (string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(request.Path, "/unauthenticated/refreshToken", StringComparison.OrdinalIgnoreCase))
                {
                    return new RefreshToken().Run(request, response);
                }
                else if (string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(request.Path, "/unauthenticated/signup", StringComparison.OrdinalIgnoreCase))
                {
                    var body = JObject.Parse(request.Body);
                    if (!body["agreedToLicense"].Value<bool>())
                    {
                        throw new Exception("agreedToLicense must be true");
                    }
                    var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
                    var userPool = new CognitoUserPool(Configuration.FINANCE_API_COGNITO_USER_POOL_ID, Configuration.FINANCE_API_COGNITO_CLIENT_ID, provider);
                    var result = userPool.SignUpAsync(
                        body["email"].Value<string>(),
                        body["password"].Value<string>(),
                        new Dictionary<string, string>(), new Dictionary<string, string>());
                    result.Wait();
                    CreateUser(body["email"].Value<string>(), body["agreedToLicense"].Value<bool>(), request.RequestContext.Identity.SourceIp);
                    json = new JObject
                    {
                        { "status", "Your user has successfully been created. " +
                                    $"Your user name is {body["email"].Value<string>()}. " +
                                    "A confirmation link has been sent to your email from noreply@primordial-software.com. " +
                                    "You need to click the verification link in the email before you can login." }
                    };
                }
                else if (string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(request.Path, "/signout", StringComparison.OrdinalIgnoreCase))
                {
                    string refreshToken = GetCookie(request, "refreshToken");
                    if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refreshToken))
                    {
                        response.StatusCode = 400;
                        response.Body = new JObject { { "error", "idToken and refreshToken cookies are required" } }.ToString();
                        return response;
                    }
                    var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
                    var userPool = new CognitoUserPool(Configuration.FINANCE_API_COGNITO_USER_POOL_ID, Configuration.FINANCE_API_COGNITO_CLIENT_ID, provider);
                    var cognitoUser = new CognitoUser(email, Configuration.FINANCE_API_COGNITO_CLIENT_ID, userPool, provider)
                    {
                        SessionTokens = new CognitoUserSession(null, null, refreshToken, DateTime.Now, DateTime.Now.AddHours(1))
                    };
                    InitiateRefreshTokenAuthRequest refreshRequest = new InitiateRefreshTokenAuthRequest
                    {
                        AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
                    };
                    var refreshResponse = cognitoUser.StartWithRefreshTokenAuthAsync(refreshRequest).Result;
                    cognitoUser.SessionTokens = new CognitoUserSession(null, refreshResponse.AuthenticationResult.AccessToken, refreshToken, DateTime.Now, DateTime.Now.AddHours(1));
                    cognitoUser.GlobalSignOutAsync().Wait();
                    response.MultiValueHeaders = new Dictionary<string, IList<string>>
                    {
                        {
                            "Set-Cookie", new List<string>
                            {
                                "idToken=;Path=/;Secure;HttpOnly;expires=Thu, 01 Jan 1970 00:00:00 UTC",
                                "refreshToken=;Path=/;Secure;HttpOnly;expires=Thu, 01 Jan 1970 00:00:00 UTC"
                            }
                        }
                    };
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
                    if (jsonPatch["licenseAgreement"] != null)
                    {
                        jsonPatch.Remove("licenseAgreement");
                    }
                    var updateItemResponse = UpdateUser(email, jsonPatch);
                    var jsonResponse = JObject.Parse(Document.FromAttributeMap(updateItemResponse.Attributes).ToJson());
                    jsonResponse.Remove("bankLinks");
                    json = jsonResponse;
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
                Console.WriteLine(exception);
                response.StatusCode = 500;
                json = new JObject {{"error", exception.ToString()}};
            }
            response.Body = json.ToString();
            return response;
        }

        public static string GetCookie(APIGatewayProxyRequest request, string cookieName)
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

        private void CreateUser(string email, bool agreedToLicense, string ip)
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
            var update = JObject.FromObject(user, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            var dbClient = new AmazonDynamoDBClient();
            dbClient.PutItemAsync(
                new FinanceUser().GetTable(),
                Document.FromJson(update.ToString()).ToAttributeMap()
            ).Wait();
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

        /// <summary>
        /// This function removes padding which is what notepad++ does as well.
        /// </summary>
        public static string Base64Decode(string base64Encoded)
        {
            base64Encoded = base64Encoded.PadRight(base64Encoded.Length + (base64Encoded.Length * 3) % 4, '=');
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));
        }
    }
}
