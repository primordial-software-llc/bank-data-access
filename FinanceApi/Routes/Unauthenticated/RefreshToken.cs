using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using Amazon.Runtime;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;

namespace FinanceApi.Routes.Unauthenticated
{
    class RefreshToken
    {
        public APIGatewayProxyResponse Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response)
        {
            string token = Function.GetCookie(request, "idToken");
            string refreshToken = Function.GetCookie(request, "refreshToken");
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refreshToken))
            {
                response.StatusCode = 400;
                response.Body = new JObject { { "error", "idToken and refreshToken cookies are required" } }.ToString();
                return response;
            }
            var payload = Function.Base64Decode(token.Split('.')[1]);
            var email = JObject.Parse(payload)["email"].Value<string>();
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
            var userPool = new CognitoUserPool(Configuration.FINANCE_API_COGNITO_USER_POOL_ID, Configuration.FINANCE_API_COGNITO_CLIENT_ID, provider);
            var cognitoUser = new CognitoUser(email, Configuration.FINANCE_API_COGNITO_CLIENT_ID, userPool, provider)
            {
                SessionTokens = new CognitoUserSession(null, null, refreshToken, DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
            };
            InitiateRefreshTokenAuthRequest refreshRequest = new InitiateRefreshTokenAuthRequest
            {
                AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
            };
            var refreshResponse = cognitoUser.StartWithRefreshTokenAuthAsync(refreshRequest).Result;
            var expirationDate = DateTime.UtcNow.AddDays(30).ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");
            response.MultiValueHeaders = new Dictionary<string, IList<string>>
                { { "Set-Cookie", new List<string> { $"idToken={refreshResponse.AuthenticationResult.IdToken};Path=/;Secure;HttpOnly;Expires={expirationDate}" } } };

            response.Body = new JObject().ToString();
            return response;
        }
    }
}
