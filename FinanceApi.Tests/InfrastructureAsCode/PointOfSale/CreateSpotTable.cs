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
            var dbClient = new DatabaseClient<Section>(client);

            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 7",
                Spots = new List<Spot>()
            };

            for (var ct = 240; ct <= 279; ct += 1)
            {
                section.Spots.Add(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ct.ToString()
                });
            }

            dbClient.Create(section);
        }

        //[Fact]
        public void CreateSpotsBuilding6()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Section>(client);

            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 6",
                Spots = new List<Spot>()
            };

            for (var ct = 200; ct <= 239; ct += 1)
            {
                section.Spots.Add(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = ct.ToString()
                });
            }

            dbClient.Create(section);
        }

        //[Fact]
        public void CreateSpotsBuilding5()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Section>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(), Name = "Building 5", Spots = new List<Spot>()
            };
            for (var ct = 160; ct <= 199; ct += 1)
            {
                section.Spots.Add(new Spot { Id = Guid.NewGuid().ToString(), Name = ct.ToString() });
            }
            dbClient.Create(section);
        }

        //[Fact]
        public void CreateSpotsBuilding4()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Section>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 4",
                Spots = new List<Spot>()
            };
            for (var ct = 120; ct <= 159; ct += 1)
            {
                section.Spots.Add(new Spot { Id = Guid.NewGuid().ToString(), Name = ct.ToString() });
            }
            dbClient.Create(section);
        }

        //[Fact]
        public void CreateSpotsBuilding3()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Section>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 3",
                Spots = new List<Spot>()
            };
            for (var ct = 80; ct <= 119; ct += 1)
            {
                section.Spots.Add(new Spot { Id = Guid.NewGuid().ToString(), Name = ct.ToString() });
            }
            dbClient.Create(section);
        }

        [Fact]
        public void CreateSpotsBuilding2()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Section>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 2",
                Spots = new List<Spot>()
            };
            for (var ct = 40; ct <= 79; ct += 1)
            {
                section.Spots.Add(new Spot { Id = Guid.NewGuid().ToString(), Name = ct.ToString() });
            }
            dbClient.Create(section);
        }

        //[Fact]
        public void CreateSpotsBuilding1()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Section>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 1",
                Spots = new List<Spot>()
            };
            for (var ct = 20; ct <= 38; ct += 1)
            {
                section.Spots.Add(new Spot { Id = Guid.NewGuid().ToString(), Name = ct.ToString() });
            }
            dbClient.Create(section);
        }

        //[Fact]
        public void Create()
        {
            var request = new CreateTableRequest
            {
                TableName = "lakeland-mi-pueblo-sections",
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
