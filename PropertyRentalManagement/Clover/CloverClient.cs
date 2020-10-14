using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PropertyRentalManagement.Clover
{
    public class CloverClient
    {
        private string Token { get; }
        private ILogging Logger { get; }
        private string MerchantId { get; }

        public CloverClient(string token, string merchantId, ILogging logger)
        {
            Token = token;
            MerchantId = merchantId;
            Logger = logger;
        }

        public List<T> QueryAll<T>(string path)
        {
            var results = new List<T>();
            var limit = 1000;
            var offset = 0;
            List<T> elements;
            do
            {
                var delimiter = path.Contains("?") ? "&" : "?";
                var finalPath = path;
                finalPath += $"{delimiter}limit={limit}";
                finalPath += $"&offset={offset}";
                var result = Request(finalPath, HttpMethod.Get);
                var json = JObject.Parse(result);
                elements = JsonConvert.DeserializeObject<List<T>>(json["elements"].ToString());
                results.AddRange(elements);
                offset += elements.Count;
            } while (elements.Count == limit);
            return results;
        }

        public string Request(string path, HttpMethod method, string body = null)
        {
            var client = new HttpClient { BaseAddress = new Uri("https://api.clover.com") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            var relativePath = $"/v3/merchants/{MerchantId}/{path}";
            var request = new HttpRequestMessage(method, relativePath);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrWhiteSpace(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            var result = client.SendAsync(request).Result;
            var response = result.Content.ReadAsStringAsync().Result;
            if (!result.IsSuccessStatusCode)
            {
                Logger.Log($"Clover API Request Failure"
                           + $" {(int) result.StatusCode} {method} {relativePath}"
                           + $" Received {response}");
            }
            result.EnsureSuccessStatusCode();

            return response;
        }
    }
}
