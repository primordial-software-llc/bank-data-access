using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AwsDataAccess
{
    public class DatabaseClient<T> where T : IModel, new()
    {
        private IAmazonDynamoDB Client { get; }
        private ILogging Logging { get; }

        public DatabaseClient(IAmazonDynamoDB client, ILogging logging)
        {
            Client = client;
            Logging = logging;
        }

        public List<T> QueryAll(QueryRequest queryRequest)
        {
            QueryResponse queryResponse = null;
            var items = new List<T>();
            do
            {
                if (queryResponse != null)
                {
                    queryRequest.ExclusiveStartKey = queryResponse.LastEvaluatedKey;
                }
                queryResponse = Client.QueryAsync(queryRequest).Result;
                foreach (var item in queryResponse.Items)
                {
                    items.Add(JsonConvert.DeserializeObject<T>(Document.FromAttributeMap(item).ToJson()));
                }
            } while (queryResponse.LastEvaluatedKey.Any());
            return items;
        }

        public List<T> ScanAll(ScanRequest scanRequest)
        {
            ScanResponse scanResponse = null;
            var items = new List<T>();
            do
            {
                if (scanResponse != null)
                {
                    scanRequest.ExclusiveStartKey = scanResponse.LastEvaluatedKey;
                }
                scanResponse = Client.ScanAsync(scanRequest).Result;
                foreach (var item in scanResponse.Items)
                {
                    items.Add(JsonConvert.DeserializeObject<T>(Document.FromAttributeMap(item).ToJson()));
                }
            } while (scanResponse.LastEvaluatedKey.Any());
            return items;
        }

        public void Delete(T model)
        {
            Client.DeleteItemAsync(model.GetTable(), model.GetKey()).Wait();
        }

        protected virtual async Task<List<T>> GetBatchOf100(List<Dictionary<string, AttributeValue>> modelKeyBatch)
        {
            var table = new T().GetTable();
            var fullAds = new List<T>();
            var fullAdBatchKeys = new Dictionary<string, KeysAndAttributes>
            {
                {
                    table,
                    new KeysAndAttributes { Keys = modelKeyBatch }
                }
            };

            const int MAX_ATTEMPTS = 4;
            int currentAttempt = 0;
            BatchGetItemResponse response;
            do
            {
                currentAttempt += 1;
                var responseTask = Client.BatchGetItemAsync(fullAdBatchKeys).ConfigureAwait(false);
                response = await responseTask;
                fullAds.AddRange(response
                    .Responses[table]
                    .Select(x => JsonConvert.DeserializeObject<T>(Document.FromAttributeMap(x).ToJson())));

                if (response.UnprocessedKeys.Any())
                {
                    var waitTime = ExponentialBackoff.GetWaitTime(currentAttempt, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1));
                    Logging.Log($"Read capacity exceeded. Failed to get {response.UnprocessedKeys[table].Keys.Count} records. " +
                                $"Attempt {currentAttempt} of {MAX_ATTEMPTS}. Retrying in {waitTime.TotalSeconds} seconds.");
                    new Sleeper(Logging).Sleep(waitTime);
                }
            } while (response.UnprocessedKeys.Any() && currentAttempt < MAX_ATTEMPTS);

            if (currentAttempt == MAX_ATTEMPTS)
            {
                throw new Exception($"Failed to read all items after {MAX_ATTEMPTS} attempts.");
            }

            return fullAds;
        }

        public async Task<List<T>> Get(List<T> models)
        {
            var modelKeyBatches = Batcher.Batch(100, models.Select(x => x.GetKey()).ToList());
            var fullModels = new List<T>();
            foreach (var modelKeyBatch in modelKeyBatches)
            {
                var batchResponse = GetBatchOf100(modelKeyBatch).ConfigureAwait(false);
                fullModels.AddRange(await batchResponse);
            }
            return fullModels;
        }

        public T Get(T model, bool? consistentRead = null)
        {
            var dbItem = Client.GetItemAsync(model.GetTable(), model.GetKey(), consistentRead.GetValueOrDefault()).Result;
            if (!dbItem.Item.Any())
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(Document.FromAttributeMap(dbItem.Item).ToJson());
        }

        public T Create(T model)
        {
            var json = JObject.FromObject(model, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            Client.PutItemAsync(
                new T().GetTable(),
                Document.FromJson(json.ToString()).ToAttributeMap()
            ).Wait();
            return model;
        }

        public T Update(T model)
        {
            Dictionary<string, AttributeValue> key = model.GetKey();
            var updateJson = JObject.FromObject(model, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            foreach (var keyPart in key.Keys)
            {
                updateJson.Remove(keyPart);
            }
            var updates = Document.FromJson(updateJson.ToString()).ToAttributeUpdateMap(false);
            var result = Client.UpdateItemAsync(
                new T().GetTable(),
                key,
                updates,
                ReturnValue.ALL_NEW
            ).Result;
            return JsonConvert.DeserializeObject<T>(Document.FromAttributeMap(result.Attributes).ToJson());
        }

    }
}
