using System;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated
{
    class PostSignOut : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/signout";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            string idToken = CookieReader.GetCookie(request, "idToken");
            string refreshToken = CookieReader.GetCookie(request, "refreshToken");
            if (string.IsNullOrWhiteSpace(idToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", "idToken and refreshToken cookies are required" } }.ToString();
                return;
            }
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
            var userPool = new CognitoUserPool(Configuration.FINANCE_API_COGNITO_USER_POOL_ID, Configuration.FINANCE_API_COGNITO_CLIENT_ID, provider);
            var cognitoUser = new CognitoUser(user.Email, Configuration.FINANCE_API_COGNITO_CLIENT_ID, userPool, provider)
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
            response.Body = Constants.JSON_EMPTY;
        }
    }
}
