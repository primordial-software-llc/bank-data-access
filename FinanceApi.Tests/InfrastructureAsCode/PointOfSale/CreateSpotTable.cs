using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using Xunit;

namespace FinanceApi.Tests.InfrastructureAsCode.PointOfSale
{
    public class CreateSpotTable
    {

        //[Fact]
        public void CreateSpotsBuilding7()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 7"
            };
            for (var ct = 240; ct <= 279; ct += 1)
            {
                dbClient.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ct.ToString(),
                    Section = section
                });
            }
        }

        //[Fact]
        public void CreateSpotsBuilding6()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 6"
            };
            for (var ct = 200; ct <= 239; ct += 1)
            {
                dbClient.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ct.ToString(),
                    Section = section
                });
            }
        }

        //[Fact]
        public void CreateSpotsBuilding5()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 5"
            };
            for (var ct = 160; ct <= 199; ct += 1)
            {
                dbClient.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ct.ToString(),
                    Section = section
                });
            }
        }

        //[Fact]
        public void CreateSpotsBuilding4()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 4"
            };
            for (var ct = 120; ct <= 159; ct += 1)
            {
                dbClient.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ct.ToString(),
                    Section = section
                });
            }
        }

        //[Fact]
        public void CreateSpotsBuilding3()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 3"
            };
            for (var ct = 80; ct <= 119; ct += 1)
            {
                dbClient.Create(new Spot { Id = Guid.NewGuid().ToString(), Name = ct.ToString(), Section = section});
            }
        }

        //[Fact]
        public void CreateSpotsBuilding2()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 2"
            };
            for (var ct = 40; ct <= 79; ct += 1)
            {
                dbClient.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ct.ToString(),
                    Section = section
                });
            }
        }

        //[Fact]
        public void CreateSpotsBuilding1()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 1"
            };
            for (var ct = 20; ct <= 38; ct += 1)
            {
                dbClient.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ct.ToString(),
                    Section = section
                });
            }
        }

        //[Fact]
        public void Create()
        {
            var request = new CreateTableRequest
            {
                TableName = "lakeland-mi-pueblo-spots",
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
                    ReadCapacityUnits = 2,
                    WriteCapacityUnits = 1
                }
            };
            var tableFactory = new DynamoDbTableFactory(
                new AmazonDynamoDBClient(Factory.CreateCredentialsFromProfile(), Factory.HomeRegion));
            tableFactory.CreateTable(request, false);
        }
    }
}
