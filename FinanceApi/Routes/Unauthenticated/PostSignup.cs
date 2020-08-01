using System;
using System.Collections.Generic;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;
using FinanceApi.DatabaseModel;
using FinanceApi.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Unauthenticated
{
    class PostSignup : IRoute
    {
        public string HttpMethod => "POST";
        public string Path => "/unauthenticated/signup";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var model = JsonConvert.DeserializeObject<SignupModel>(request.Body);
            if (!model.AgreedToLicense)
            {
                throw new Exception("agreedToLicense must be true");
            }
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.USEast1);
            var userPool = new CognitoUserPool(Configuration.FINANCE_API_COGNITO_USER_POOL_ID, Configuration.FINANCE_API_COGNITO_CLIENT_ID, provider);
            var result = userPool.SignUpAsync(
                model.Email.ToLower(),
                model.Password,
                new Dictionary<string, string>(),
                new Dictionary<string, string>());
            result.Wait();
            var userService = new UserService();
            var ip = request.RequestContext.Identity.SourceIp;
            if (request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                ip = request.Headers["CF-Connecting-IP"];
            }
            userService.CreateUser(model.Email, model.AgreedToLicense, ip);
            var json = new JObject
            {
                { "status", "Your user has successfully been created. " +
                            $"Your user name is {model.Email}. " +
                            "A confirmation link has been sent to your email from noreply@primordial-software.com. " +
                            "You need to click the verification link in the email before you can login." }
            };
            response.Body = json.ToString();
        }
    }
}
