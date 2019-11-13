using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
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
            var client = new BankClient(PlaidConfiguration.DEV_URL);
            var response = new APIGatewayProxyResponse();
            response.Headers = new Dictionary<string, string>();
            response.Headers.Add("access-control-allow-origin", "*");
            response.StatusCode = 200;
            JObject json;
            var authHeader = request.Headers["Authorization"];
            var payload = Base64Decode(authHeader.Split('.')[1]);
            var email = JObject.Parse(payload)["email"].Value<string>();
            Console.WriteLine("User accessed API's: " + email);

            var dbClient = new AmazonDynamoDBClient();
            try
            {
                if (string.Equals(request.HttpMethod, "PATCH") &&
                    string.Equals(request.Path, "/budget"))
                {
                    var updateItemResponse = dbClient.UpdateItemAsync(
                        new BankDataAccessUser().GetTable(),
                        BankDataAccessUser.GetKey(email),
                        Document.FromJson(request.Body).ToAttributeUpdateMap(false),
                        ReturnValue.ALL_NEW
                    ).Result;
                    json = JObject.Parse(Document.FromAttributeMap(updateItemResponse.Attributes).ToJson());
                    response.StatusCode = (int) updateItemResponse.HttpStatusCode;
                }
                else if (string.Equals(request.HttpMethod, "GET") &&
                         string.Equals(request.Path, "/budget"))
                {
                    var dbItem = dbClient.GetItemAsync(new BankDataAccessUser().GetTable(),
                        new BankDataAccessUser {Email = email}.GetKey()
                    ).Result;
                    // MOVE THIS TO AWSTOOLS
                    var user = JsonConvert.DeserializeObject<BankDataAccessUser>(Document.FromAttributeMap(dbItem.Item).ToJson());
                    user.AccessTokens = null; // Don't send this data to the client. It should get moved to another table that's encrypted.
                    json = JObject.Parse(JsonConvert.SerializeObject(user));
                }
                else if (string.Equals(request.HttpMethod, "GET") &&
                         string.Equals(request.Path, "/accountBalance"))
                {
                    var dbItem = dbClient.GetItemAsync(new BankDataAccessUser().GetTable(), new BankDataAccessUser { Email = email }.GetKey()).Result;
                    var user = JsonConvert.DeserializeObject<BankDataAccessUser>(Document.FromAttributeMap(dbItem.Item).ToJson());
                    var accounts = new JArray();
                    foreach (var accessToken in user.AccessTokens ?? new List<string>())
                    {
                        var institutionAccounts = client.GetAccounts(accessToken);
                        foreach (var account in institutionAccounts["accounts"])
                        {
                            accounts.Add(account);
                        }
                    }
                    json = new JObject {{"accounts", accounts}};
                }
                else
                {
                    throw new Exception("Unknown requestType");
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
        private string Base64Decode(string base64Encoded)
        {
            base64Encoded = base64Encoded.PadRight(base64Encoded.Length + (base64Encoded.Length * 3) % 4, '=');
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));
        }
    }
}
