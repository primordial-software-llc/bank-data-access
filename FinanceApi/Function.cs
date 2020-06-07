using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FinanceApi.DatabaseModel;
using FinanceApi.Routes;
using FinanceApi.Routes.Authenticated;
using FinanceApi.Routes.Authenticated.PointOfSale;
using FinanceApi.Routes.Unauthenticated;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace FinanceApi
{
    public class Function
    {
        public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
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
            string idToken = CookieReader.GetCookie(request, "idToken");
            bool isAuthenticated = false;
            if (!request.Path.StartsWith("/unauthenticated/", StringComparison.OrdinalIgnoreCase))
            {
                isAuthenticated = AwsCognitoJwtTokenValidator.IsValid(idToken, Configuration.FINANCE_API_COGNITO_USER_POOL_ID);
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
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                var payload = Base64Decode(idToken.Split('.')[1]);
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
                List<IRoute> routes = new List<IRoute>
                {
                    new PostSetToken(),
                    new PostRefreshToken(),
                    new PostSignup(),
                    new PostSignOut(),
                    new PostPurchase(),
                    new DeleteBankLink(),
                    new GetBankLink(),
                    new PostCreatePublicToken(),
                    new PostLinkAccessToken(),
                    new GetAccountBalance(),
                    new GetBudget(),
                    new PatchBudget(),
                    new GetCustomers()
                };

                var matchedRoute = routes.FirstOrDefault(route => string.Equals(request.HttpMethod, route.HttpMethod, StringComparison.OrdinalIgnoreCase) &&
                                                                  string.Equals(request.Path, route.Path, StringComparison.OrdinalIgnoreCase));
                if (matchedRoute != null)
                {
                    matchedRoute.Run(request, response, user);
                }
                else
                {
                    response.StatusCode = 404;
                    response.Body = Constants.JSON_EMPTY;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                response.StatusCode = 500;
                response.Body = new JObject {{"error", exception.ToString()}}.ToString();
            }
            return response;
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
