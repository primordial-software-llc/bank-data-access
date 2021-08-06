using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using AwsDataAccess;
using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline
{
    public class OAuthClient
    {
        public static QuickBooksOnlineBearerToken GetAccessToken(string clientId, string clientSecret, string refreshToken, ILogging logger)
        {
            var authBasic = $"{clientId}:{clientSecret}";
            var authBasicEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(authBasic));
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/oauth2/v1/tokens/bearer");
            var nvc = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            };
            request.Content = new FormUrlEncodedContent(nvc);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var client = new HttpClient {BaseAddress = new Uri("https://oauth.platform.intuit.com")};
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authBasicEncoded);
            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
                 
            {
                logger.Log(result);
            }
            response.EnsureSuccessStatusCode();
            var bearerToken = JsonConvert.DeserializeObject<QuickBooksOnlineBearerToken>(result);
            return bearerToken;
        }
    }
}
