using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using AwsTools;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FinanceApi
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
    }
}
