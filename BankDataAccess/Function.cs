using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
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

            try
            {
                json = client.GetAccountBalance();
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
