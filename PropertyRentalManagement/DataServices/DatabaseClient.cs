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

        public List<T> GetAll()
        {
            var scanRequest = new ScanRequest(new T().GetTable());
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
                    items.Add(JsonConvert.DeserializeObject<T>(Document.FromAttributeMap(item)));
                }
            } while (scanResponse.LastEvaluatedKey.Any());
            return items;
        }

        public T Get(T model)
        {
            var dbItem = Client.GetItemAsync(model.GetTable(), model.GetKey()).Result;
            return JsonConvert.DeserializeObject<T>(Document.FromAttributeMap(dbItem.Item).ToJson());
        }

        public void Create(T model)
        {
            var json = JObject.FromObject(model, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            Client.PutItemAsync(
                new T().GetTable(),
                Document.FromJson(json.ToString()).ToAttributeMap()
            ).Wait();
        }

        public void Update(Dictionary<string, AttributeValue> key, T model)
        {
            var update = JObject.FromObject(model, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            var updates = Document.FromJson(update.ToString()).ToAttributeUpdateMap(false);
            Client.UpdateItemAsync(
                new T().GetTable(),
                key,
                updates
            ).Wait();
        }
    }
}
