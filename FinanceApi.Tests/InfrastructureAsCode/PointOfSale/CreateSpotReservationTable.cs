using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AwsDataAccess;
using PropertyRentalManagement.DatabaseModel;
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

            var sectionId = "2d2fa812-3bcb-4955-9a7c-63922e7392fa";
            var sectionName = "Field I";

            for (var index = 1; index <= 7; index += 1)
            {
                client.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Walkway",
                    Section = new Section
                    {
                        Id = sectionId,
                        Name = sectionName
                    }
                });
            }
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
