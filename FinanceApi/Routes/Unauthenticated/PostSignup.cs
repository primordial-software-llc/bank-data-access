using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Unauthenticated
{
    class PostSignup : IRoute
    {
        public APIGatewayProxyResponse Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user = null)
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
            var json = new JObject
            {
                { "status", "Your user has successfully been created. " +
                            $"Your user name is {body["email"].Value<string>()}. " +
                            "A confirmation link has been sent to your email from noreply@primordial-software.com. " +
                            "You need to click the verification link in the email before you can login." }
            };
            response.Body = json.ToString();
            return response;
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
    }
}
