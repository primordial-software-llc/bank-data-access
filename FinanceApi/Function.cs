using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FinanceApi.DatabaseModel;
using FinanceApi.Routes;
using FinanceApi.Routes.Authenticated.PointOfSale;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.DataServices;

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
                isAuthenticated = AwsCognitoJwtTokenValidator.IsValid(idToken, Configuration.FINANCE_API_COGNITO_USER_POOL_ID, Configuration.FINANCE_API_COGNITO_CLIENT_ID);
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
                    if (user == null)
                    {
                        throw new Exception($"User not found {email}. Use the correct sign up form at https://www.primordial-software.com/pages/login-signup.html");
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
                    new Routes.Unauthenticated.PostSetToken(),
                    new Routes.Unauthenticated.PostRefreshToken(),
                    new Routes.Unauthenticated.PostSignup(),
                    new Routes.Unauthenticated.PointOfSale.GetSpotsForPublic()
                };
                if ((user?.BillingAgreement?.AgreedToBillingTerms).GetValueOrDefault())
                {
                    routes.Add(new Routes.Authenticated.PaidFeatures.PostLinkAccessToken());
                    routes.Add(new Routes.Authenticated.PaidFeatures.GetBankLink());
                    routes.Add(new Routes.Authenticated.PaidFeatures.DeleteBankLink());
                    routes.Add(new Routes.Authenticated.PaidFeatures.PostCreatePublicToken());
                    routes.Add(new Routes.Authenticated.PaidFeatures.GetBankTransactions());
                }
                if (isAuthenticated)
                {
                    routes.Add(new Routes.Authenticated.PostSignOut());
                    routes.Add(new Routes.Authenticated.PostPurchase());
                    routes.Add(new Routes.Authenticated.GetAccountBalance());
                    routes.Add(new Routes.Authenticated.GetBudget());
                    routes.Add(new Routes.Authenticated.PatchBudget());
                }
                if (new PointOfSaleAuthorization().IsAuthorized(user?.Email))
                {
                    routes.Add(new Routes.Authenticated.PointOfSale.GetCustomers());
                    routes.Add(new Routes.Authenticated.PointOfSale.GetSpots());
                    routes.Add(new Routes.Authenticated.PointOfSale.GetSpotReservations());
                    routes.Add(new Routes.Authenticated.PointOfSale.GetCustomerPaymentSettings());
                    routes.Add(new Routes.Authenticated.PointOfSale.GetCustomerPaymentSettingsById());
                    routes.Add(new Routes.Authenticated.PointOfSale.GetCustomerInvoices());
                    routes.Add(new Routes.Authenticated.PointOfSale.GetCashBasisIncome());
                    routes.Add(new Routes.Authenticated.PointOfSale.PatchVendor());
                    routes.Add(new Routes.Authenticated.PointOfSale.PatchSpot());
                    routes.Add(new Routes.Authenticated.PointOfSale.DeleteReservation());
                    routes.Add(new Routes.Authenticated.PointOfSale.PostReceipt());
                    routes.Add(new Routes.Authenticated.PointOfSale.PostCreateWeeklyInvoices());
                    routes.Add(new Routes.Authenticated.PointOfSale.PostCreateMonthlyInvoices());
                    routes.Add(new Routes.Authenticated.PointOfSale.GetRecurringInvoiceDateRange());
                }
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
