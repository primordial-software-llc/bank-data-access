using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using AwsTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Amazon.DynamoDBv2.Model;

namespace PropertyRentalManagement
{
    public class DatabaseClient<T> where T : IModel, new()
    {
        private IAmazonDynamoDB Client { get; }

        public DatabaseClient(IAmazonDynamoDB client)
        {
            Client = client;
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
