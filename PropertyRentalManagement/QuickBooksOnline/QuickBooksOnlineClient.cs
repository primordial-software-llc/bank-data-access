using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using AwsDataAccess;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline.Models;
using static PropertyRentalManagement.BusinessLogic.RecurringInvoices;

namespace PropertyRentalManagement.QuickBooksOnline
{
    public class QuickBooksOnlineClient
    {
        private QuickBooksOnlineBearerToken Token { get; set; }
        private ILogging Logger { get; }
        private string RealmId { get; }
        private DatabaseClient<QuickBooksOnlineConnection> DbClient { get; }

        public QuickBooksOnlineClient(string realmId, DatabaseClient<QuickBooksOnlineConnection> dbClient, ILogging logger)
        {
            DbClient = dbClient;
            RealmId = realmId;
            Logger = logger;
            var qboConnection = dbClient.Get(new QuickBooksOnlineConnection { RealmId = realmId }, true);
            Token = OAuthClient.GetAccessToken(
                qboConnection.ClientId,
                qboConnection.ClientSecret,
                qboConnection.RefreshToken,
                logger);
            dbClient.Update(
                new QuickBooksOnlineConnection
                {
                    RealmId = qboConnection.RealmId,
                    AccessToken = Token.AccessToken,
                    RefreshToken = Token.RefreshToken
                });
            // Token will get stale during process, more work needs to be done, but that is why the token isn't injected. Token has to be retreived in the client to do the refresh mid-process.
        }

        public QuickBooksOnlineConnection GetConnectionForLocks()
        {
            return DbClient.Get(new QuickBooksOnlineConnection { RealmId = RealmId }, true);
        }

        public void LockInvoices(Frequency frequency, bool lockUpdateType)
        {
            if (frequency == Frequency.Weekly)
            {
                LockWeeklyInvoices(lockUpdateType);
            }
            else if (frequency == Frequency.Monthly)
            {
                LockMonthlyInvoices(lockUpdateType);
            }
            else
            {
                throw new Exception($"Can't lock invoices due to unknown frequency of {frequency}");
            }
        }

        public void LockWeeklyInvoices(bool lockUpdateType)
        {
            DbClient.Update(
                new QuickBooksOnlineConnection
                {
                    RealmId = RealmId,
                    WeeklyInvoiceLock = lockUpdateType
                });
        }

        public void LockMonthlyInvoices(bool lockUpdateType)
        {
            DbClient.Update(
                new QuickBooksOnlineConnection
                {
                    RealmId = RealmId,
                    MonthlyInvoiceLock = lockUpdateType
                });
        }

        public int QueryCount<T>(string query) where T : IQuickBooksOnlineEntity, new()
        {
            var result = Request($"query?query={HttpUtility.UrlEncode(query)}", HttpMethod.Get);
            var json = JObject.Parse(result);
            return json["QueryResponse"]["totalCount"].Value<int>();
        }

        public List<T> QueryAll<T>(string query) where T : IQuickBooksOnlineEntity, new()
        {
            var count = QueryCount<T>(query.Replace("select * from", "select count(*) from"));
            var maxResults = 1000;
            var allResults = new List<T>();
            for (int startPosition = 1; startPosition <= count; startPosition += maxResults)
            {
                var pagedQuery = $"{query} STARTPOSITION {startPosition} MAXRESULTS {maxResults}";
                var result = Request($"query?query={HttpUtility.UrlEncode(pagedQuery)}", HttpMethod.Get);
                var json = JObject.Parse(result);
                var entityResults = json["QueryResponse"][new T().EntityName];
                allResults.AddRange(JsonConvert.DeserializeObject<IList<T>>(entityResults.ToString()));
            }

            return allResults;
        }

        public List<T> Query<T>(string query) where T : IQuickBooksOnlineEntity, new()
        {
            var result = Request($"query?query={HttpUtility.UrlEncode(query)}", HttpMethod.Get);
            var json = JObject.Parse(result);
            var entityResults = json["QueryResponse"][new T().EntityName];
            if (entityResults == null)
            {
                return new List<T>();
            }
            return JsonConvert.DeserializeObject<List<T>>(entityResults.ToString());
        }

        public T Create<T>(T model) where T : IQuickBooksOnlineEntity, new()
        {
            model.Id = null;
            var jsonInput = JsonConvert.SerializeObject(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var result = Request(model.EntityName.ToLower(), HttpMethod.Post, jsonInput);
            var json = JObject.Parse(result);
            return JsonConvert.DeserializeObject<T>(json[new T().EntityName].ToString());
        }

        public T Delete<T>(T model) where T : IQuickBooksOnlineEntity, new()
        {
            var jsonInput = JsonConvert.SerializeObject(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var result = Request(model.EntityName.ToLower() + "?operation=delete", HttpMethod.Post, jsonInput);
            var json = JObject.Parse(result);
            return JsonConvert.DeserializeObject<T>(json[new T().EntityName].ToString());
        }

        public T FullUpdate<T>(T model) where T : IQuickBooksOnlineEntity, new()
        {
            model.Sparse = false;
            var jsonInput = JsonConvert.SerializeObject(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var result = Request(model.EntityName.ToLower(), HttpMethod.Post, jsonInput);
            var json = JObject.Parse(result);
            return JsonConvert.DeserializeObject<T>(json[new T().EntityName].ToString());
        }

        public T SparseUpdate<T>(T model) where T : IQuickBooksOnlineEntity, new()
        {
            model.Sparse = true;
            var jsonInput = JsonConvert.SerializeObject(model, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var result = Request(model.EntityName.ToLower(), HttpMethod.Post, jsonInput);
            var json = JObject.Parse(result);
            return JsonConvert.DeserializeObject<T>(json[new T().EntityName].ToString());
        }

        public string Request(string path, HttpMethod method, string body = null)
        {
            var client = new HttpClient { BaseAddress = new Uri("https://quickbooks.api.intuit.com") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.AccessToken);

            var relativePath = $"/v3/company/{RealmId}/{path}";
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
                Logger.Log($"QuickBooks Online API Request Failure"
                           + $" {(int) result.StatusCode} {method} {relativePath}"
                           + $" Received {response}");
            }
            result.EnsureSuccessStatusCode();

            return response;
        }
    }
}
