using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BankDataAccess
{
    public class BankClient
    {
        private string BaseUrl { get; }
        private readonly HttpClient Client = new HttpClient();

        public BankClient(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        /// <summary>
        /// Get account balance doesn't work with capital one: /accounts/balance/get
        /// </summary>
        public JObject GetAccounts(string accessToken)
        {
            var data = new JObject
            {
                {"client_id", PlaidConfiguration.DEV_CLIENT_ID },
                {"secret", PlaidConfiguration.DEV_SECRET },
                { "access_token", accessToken }
            };
            var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            var url = BaseUrl + "/accounts/get";
            var postResult = Client.PostAsync(url, content).Result;
            var result = postResult.Content.ReadAsStringAsync().Result;
            if (!postResult.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }
            return JObject.Parse(result);
        }
    }
}
