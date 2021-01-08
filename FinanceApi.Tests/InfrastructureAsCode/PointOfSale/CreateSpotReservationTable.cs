using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using Xunit;

namespace FinanceApi.Tests.InfrastructureAsCode.PointOfSale
{
    public class CreateSpotReservationTable
    {
        [Fact]
        public void CreateSpots()
        {
            var client = new DatabaseClient<Spot>(
                new AmazonDynamoDBClient(Factory.CreateCredentialsFromProfile(), Factory.HomeRegion),
                new ConsoleLogger());

            /*
            client.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "North Walkway",
                Section = new Section
                {
                    Id = "47afac0b-67c2-4807-a88d-3ea9e1775661",
                    Name = "Field H"
                }
            });
            */

            /*
            client.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "2d2fa812-3bcb-4955-9a7c-63922e7392fa",
                    Name = "Field I"
                }
            });
            */
            /*
            client.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "596c8ac6-ebf9-4438-9973-4a516288d7b9",
                    Name = "Field J"
                }
            });
            */
            /*
            client.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "e8651c71-e5dd-4706-a18c-d2ee0e0da00c",
                    Name = "Field K"
                }
            });
            */
            /*
            client.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "3408b26a-b7ed-4e76-8a42-9b574181afae",
                    Name = "Field L"
                }
            });
            */
        }

        //[Fact]
        public void A_Create()
        {
            var request = new CreateTableRequest
            {
                TableName = new SpotReservation().GetTable(),
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "rentalDate",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "spotId",
                        KeyType = "RANGE"
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "spotId",
                        AttributeType = "S"
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "rentalDate",
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
