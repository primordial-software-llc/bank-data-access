using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using AwsTools;
using Newtonsoft.Json;

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
    }
}
