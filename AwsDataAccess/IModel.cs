using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace AwsDataAccess
{
    public interface IModel
    {
        Dictionary<string, AttributeValue> GetKey();
        string GetTable();
    }
}
