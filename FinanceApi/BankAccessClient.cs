using System;
using System.Net.Http;
using System.Text;
using AwsTools;
using FinanceApi.PlaidModel;
using FinanceApi.RequestModels;
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
                { "client_id", Configuration.PlaidClientId },
                { "secret", Configuration.PlaidSecret },
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
                { "public_key", Configuration.PlaidPublicKey }
            };
            return Send("/institutions/get_by_id", data);
        }

        public JObject GetItem(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", Configuration.PlaidClientId },
                { "secret", Configuration.PlaidSecret },
                { "access_token", accessToken }
            };
            return Send("/item/get", data);
        }

        public JObject RemoveItem(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", Configuration.PlaidClientId },
                { "secret", Configuration.PlaidSecret },
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
                { "client_id", Configuration.PlaidClientId },
                { "access_token", accessToken },
                { "secret", Configuration.PlaidSecret }
            };
            return Send("/item/public_token/create", data);
        }

        public AccessTokenResponse GetAccessToken(string publicToken)
        {
            var data = new JObject
            {
                { "client_id", Configuration.PlaidClientId },
                { "public_token", publicToken },
                { "secret", Configuration.PlaidSecret }
            };
            var json = Send("/item/public_token/exchange", data);
            return JsonConvert.DeserializeObject<AccessTokenResponse>(json.ToString());
        }

        public JObject GetDwollaProcessorToken(string accessToken, string accountId)
        {
            var data = new JObject
            {
                { "client_id", Configuration.PlaidClientId },
                { "secret", Configuration.PlaidSecret },
                { "access_token", accessToken },
                { "account_id", accountId },
                { "processor", "dwolla" }
            };
            return Send("/processor/token/create", data);
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
