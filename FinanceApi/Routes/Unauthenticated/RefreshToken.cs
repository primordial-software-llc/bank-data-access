using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using Amazon.Runtime;
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
using FinanceApi.Routes.Unauthenticated;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Unauthenticated
{
    class RefreshToken
    {
        public APIGatewayProxyResponse Run(string email, string refreshToken, APIGatewayProxyResponse response)
        {
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
            response.MultiValueHeaders = new Dictionary<string, IList<string>>
                { { "Set-Cookie", new List<string> { $"idToken={refreshResponse.AuthenticationResult.IdToken};Path=/;Secure;HttpOnly" } } };

            response.Body = new JObject().ToString();
            return response;
        }
    }
}
