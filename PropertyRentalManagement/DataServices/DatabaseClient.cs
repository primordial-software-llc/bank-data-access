using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PropertyRentalManagement.DataServices
{
    public class DatabaseClient<T> where T : IModel, new()
    {
        private IAmazonDynamoDB Client { get; }

        public DatabaseClient(IAmazonDynamoDB client)
        {
            Client = client;
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
