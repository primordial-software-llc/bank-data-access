using System;
using System.Collections.Generic;
using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace FinanceApi.Tests.InfrastructureAsCode
{
    class DynamoDbTableFactory
    {
        private AmazonDynamoDBClient Client { get; }

        public DynamoDbTableFactory(AmazonDynamoDBClient client)
        {
            Client = client;
        }

        public void CreateTable(CreateTableRequest request)
        {
            TableDescription tableDescription;
            var tableExists = TableExists(request.TableName);
            if (tableExists)
            {
                Console.WriteLine("Table found, deleting");
                DeleteTableResponse deleteResponse = Client.DeleteTableAsync(request.TableName).Result;
                if (HttpStatusCode.OK != deleteResponse.HttpStatusCode)
                {
                    throw new Exception("Table delete failed: " + deleteResponse.HttpStatusCode);
                }

                tableDescription = Client.DescribeTableAsync(request.TableName).Result.Table;
                if (tableDescription.TableStatus != TableStatus.DELETING)
                {
                    throw new Exception("Table isn't deleting after delete was issued. Current status: " + tableDescription.TableStatus);
                }

                do
                {
                    System.Threading.Thread.Sleep(200);
                    tableExists = TableExists(request.TableName);
                    Console.WriteLine("Table found after deleting, waiting.");
                } while (tableExists);
            }

            CreateTableResponse response = Client.CreateTableAsync(request).Result;
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Create table failed: " + response.HttpStatusCode);
            }

            tableDescription = Client.DescribeTableAsync(request.TableName).Result.Table;
            if (tableDescription.TableStatus != TableStatus.CREATING)
            {
                throw new Exception("Table isn't creating after create was issued. Current status: " + tableDescription.TableStatus);
            }
            WaitForTableStatus(request.TableName, TableStatus.ACTIVE);
        }

        private bool TableExists(string tableName)
        {
            bool tableExists = false;
            TryCatch.Try(
            tryAction: () =>
            {
                DescribeTableResponse response = Client.DescribeTableAsync(tableName).Result;
                tableExists = true;
            },
            catchAction: caughtException =>
            {
                tableExists = false;
            },
            catchableExceptions: new List<Type> { typeof(ResourceNotFoundException) });
            return tableExists;
        }

        private void WaitForTableStatus(string tableName, TableStatus status)
        {
            TableDescription tableDescription;
            do
            {
                System.Threading.Thread.Sleep(200);
                tableDescription = Client.DescribeTableAsync(tableName).Result.Table;
                Console.WriteLine("Waiting for table status: " + status.Value);
            } while (tableDescription.TableStatus != status);
        }
    }
}
