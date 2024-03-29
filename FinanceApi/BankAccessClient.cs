﻿using System;
using System.Net.Http;
using System.Text;
using AwsDataAccess;
using FinanceApi.PlaidModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi
{
    public class BankAccessClient
    {
        private string BaseUrl { get; }
        private string ClientId { get; }
        private string Secret { get; }
        private ILogging Logger { get; }
        private readonly HttpClient Client = new HttpClient();
        

        public BankAccessClient(
            string baseUrl,
            string clientId,
            string secret,
            ILogging logger)
        {
            BaseUrl = baseUrl;
            ClientId = clientId;
            Secret = secret;
            Logger = logger;
        }

        public TransactionsResponse GetTransactions(string accessToken, string startDate, string endDate, int count, int offset)
        {
            var data = new JObject
            {
                { "client_id", ClientId },
                { "secret", Secret },
                { "access_token", accessToken },
                { "start_date", startDate },
                { "end_date", endDate },
                {
                    "options", new JObject
                    {
                        { "count", count },
                        { "offset", offset }
                    }
                }
            };
            var json = Send("/transactions/get", data);
            return JsonConvert.DeserializeObject<TransactionsResponse>(json.ToString());
        }

        public AccountBalance GetAccountBalance(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", ClientId },
                { "secret", Secret },
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
                { "client_id", ClientId },
                { "secret", Secret },
                { "country_codes", new JArray { "US" }}
            };
            return Send("/institutions/get_by_id", data);
        }

        public JObject GetItem(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", ClientId },
                { "secret", Secret },
                { "access_token", accessToken }
            };
            return Send("/item/get", data);
        }

        public JObject RemoveItem(string accessToken)
        {
            var data = new JObject
            {
                { "client_id", ClientId },
                { "secret", Secret },
                { "access_token", accessToken }
            };
            var result = Send("/item/remove", data);
            if (!result["removed"].Value<bool>())
            {
                throw new Exception("Failed to remove item: " + result);
            }
            return result;
        }

        public ExchangeAccessToken GetAccessToken(string publicToken)
        {
            var data = new JObject
            {
                { "client_id", ClientId },
                { "public_token", publicToken },
                { "secret", Secret }
            };
            var response = Send("/item/public_token/exchange", data);
            return JsonConvert.DeserializeObject<ExchangeAccessToken>(response.ToString());
        }

        public JObject Send(string path, JObject data)
        {
            return Send(path, data.ToString());
        }

        public JObject Send<T>(string path, T data)
        {
            return Send(path, JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        private JObject Send(string path, string data)
        {
            var url = BaseUrl + path;
            var content = new StringContent(data, Encoding.UTF8, "application/json");
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
