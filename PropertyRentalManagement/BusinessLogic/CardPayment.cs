using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using AwsTools;
using FinanceApi.CloverModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PropertyRentalManagement.BusinessLogic
{
    public class CardPayment
    {
        private ILogging Logger { get; }
        private string CloverPrivateToken { get; }

        public const string CLOVER_PROD_ECOMMERCE_API = "https://scl.clover.com";

        public CardPayment(ILogging logger, string cloverPrivateToken)
        {
            Logger = logger;
            CloverPrivateToken = cloverPrivateToken;
        }

        public JObject Capture(string chargeId)
        {
            var httpClient = new HttpClient();
            var endpoint = $"{CLOVER_PROD_ECOMMERCE_API}/v1/charges/{chargeId}/capture";
            var captureRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CloverPrivateToken);
            captureRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            captureRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");
            var response = httpClient.SendAsync(captureRequest).Result;
            var body = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                Logger.Log($"Clover API Request Failure {(int)response.StatusCode} {HttpMethod.Post} {endpoint} Received {body}");
            }
            response.EnsureSuccessStatusCode();
            return JObject.Parse(body);
        }

        public JObject Authorize(
            decimal? amount,
            string cardNumber,
            string expirationMonth,
            string expirationYear,
            string cvv)
        {
            var httpClient = new HttpClient();
            var apiKeyRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.clover.com/pakms/apikey");
            apiKeyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CloverPrivateToken);
            apiKeyRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var apiKeyResult = httpClient.SendAsync(apiKeyRequest).Result;
            var apiKeyResponse = apiKeyResult.Content.ReadAsStringAsync().Result;
            if (!apiKeyResult.IsSuccessStatusCode)
            {
                Logger.Log("Clover API Request Failure"
                           + $" {(int)apiKeyResult.StatusCode} {HttpMethod.Get} https://api.clover.com/pakms/apikey"
                           + $" Received {apiKeyResponse}");
            }
            apiKeyResult.EnsureSuccessStatusCode();
            var apiAccessKeyResult = JObject.Parse(apiKeyResponse);
            var tokenizeCardRequest = new TokenizeCardRequest
            {
                Card = new Card
                {
                    Number = cardNumber,
                    ExpirationMonth = expirationMonth,
                    ExpirationYear = expirationYear,
                    Cvv = cvv
                }
            };
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://token.clover.com/v1/tokens");
            tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            tokenRequest.Headers.Add("apikey", apiAccessKeyResult["apiAccessKey"].Value<string>());
            tokenRequest.Content = new StringContent(JsonConvert.SerializeObject(tokenizeCardRequest, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");
            var tokenResult = httpClient.SendAsync(tokenRequest).Result;
            var tokenResponse = tokenResult.Content.ReadAsStringAsync().Result;
            if (!tokenResult.IsSuccessStatusCode)
            {
                Logger.Log($"Clover API Request Failure"
                           + $" {(int)tokenResult.StatusCode} {HttpMethod.Post} https://token.clover.com/v1/tokens"
                           + $" Received {tokenResponse}");
            }
            var tokenResultJson = JObject.Parse(tokenResponse);
            if (tokenResultJson["error"] != null)
            {
                return tokenResultJson;
            }
            tokenResult.EnsureSuccessStatusCode();
            var sourceToken = tokenResultJson["id"].Value<string>();
            var chargeData = new JObject
            {
                { "amount", amount },
                { "source", sourceToken },
                { "capture", false },
                { "currency", "usd" }
            };
            var chargeRequest = new HttpRequestMessage(HttpMethod.Post, $"{CLOVER_PROD_ECOMMERCE_API}/v1/charges");
            chargeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CloverPrivateToken);
            chargeRequest.Content = new StringContent(chargeData.ToString(), Encoding.UTF8, "application/json");
            var chargeResult = httpClient.SendAsync(chargeRequest).Result;
            var chargeResponse = chargeResult.Content.ReadAsStringAsync().Result;
            if (!chargeResult.IsSuccessStatusCode)
            {
                Logger.Log($"Clover API Request Failure"
                           + $" {(int)chargeResult.StatusCode} {HttpMethod.Post} {CLOVER_PROD_ECOMMERCE_API}/v1/charges"
                           + $" Received {chargeResponse}");
            }
            var chargeResponseJson = JObject.Parse(chargeResponse);
            if (chargeResponseJson["error"] != null)
            {
                return chargeResponseJson;
            }
            chargeResult.EnsureSuccessStatusCode();
            return chargeResponseJson;
        }
    }
}
