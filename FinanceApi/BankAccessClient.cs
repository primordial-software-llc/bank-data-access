using System;
using System.Net.Http;
using System.Text;
using AwsTools;
using FinanceApi.PlaidModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi
{
    public class BankAccessClient
    {
        private string BaseUrl { get; }
        private ILogging Logger { get; }
        private readonly HttpClient Client = new HttpClient();
        

        public BankAccessClient(string baseUrl, ILogging logger)
        {
            BaseUrl = baseUrl;
            Logger = logger;
        }

        public AccountBalance GetAccountBalance(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", Configuration.DEV_CLIENT_ID },
                { "secret", Configuration.DEV_SECRET },
                { "access_token", accessToken }
            };
            var json = Send("/accounts/balance/get", data);
            return JsonConvert.DeserializeObject<AccountBalance>(json.ToString());
        }

        public JObject GetInstitution(string institutionId)
        {
            var data = new JObject
            {
                { "institution_id", institutionId },
                { "public_key", Configuration.DEV_PUBLIC_KEY }
            };
            return Send("/institutions/get_by_id", data);
        }

        public JObject GetItem(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", Configuration.DEV_CLIENT_ID },
                { "secret", Configuration.DEV_SECRET },
                { "access_token", accessToken }
            };
            return Send("/item/get", data);
        }

        public JObject RemoveItem(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", Configuration.DEV_CLIENT_ID },
                { "secret", Configuration.DEV_SECRET },
                { "access_token", accessToken }
            };
            var result = Send("/item/remove", data);
            if (!result["removed"].Value<bool>())
            {
                throw new Exception("Failed to remove item: " + result);
            }
            return result;
        }

        public JObject CreatePublicToken(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", Configuration.DEV_CLIENT_ID },
                { "access_token", accessToken },
                { "secret", Configuration.DEV_SECRET }
            };
            return Send("/item/public_token/create", data);
        }

        public JObject GetAccessToken(string publicToken)
        {
            var data = new JObject
            {
                { "client_id", Configuration.DEV_CLIENT_ID },
                { "public_token", publicToken },
                { "secret", Configuration.DEV_SECRET }
            };
            return Send("/item/public_token/exchange", data);
        }

        private JObject Send(string path, JObject data)
        {
            var url = BaseUrl + path;
            var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            var postResult = Client.PostAsync(url, content).Result;
            var result = postResult.Content.ReadAsStringAsync().Result;
            if (!postResult.IsSuccessStatusCode)
            {
                var received = ((int) postResult.StatusCode) + " - " + result;
                var msg = $"Failed to send to: {url}" + Environment.NewLine +
                          $"Received: {received}" + Environment.NewLine +
                          "Sent: " + data;
                Logger.Log(msg);
                throw new Exception(received);
            }
            return JObject.Parse(result);
        }

    }
}
