﻿using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode.PointOfSale
{
    public class CreateReceiptsTable
    {
        private ITestOutputHelper Output { get; }

        public CreateReceiptsTable(ITestOutputHelper output)
        {
            Output = output;
        }

        //[Fact]
        public void Create()
        {
            var request = new CreateTableRequest
            {
                TableName = "lakeland-mi-pueblo-receipts",
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "id",
                        KeyType = "HASH"
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "id",
                        AttributeType = "S"
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 5,
                    WriteCapacityUnits = 1
                }
            };
            var tableFactory = new DynamoDbTableFactory(
                new AmazonDynamoDBClient(Factory.CreateCredentialsFromProfile(), Factory.HomeRegion));
            tableFactory.CreateTable(request, false);
        }
    }
}
