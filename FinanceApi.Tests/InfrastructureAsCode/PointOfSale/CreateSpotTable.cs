using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode.PointOfSale
{
    public class CreateSpotTable
    {
        private ITestOutputHelper Output { get; }

        public CreateSpotTable(ITestOutputHelper output)
        {
            Output = output;
        }

        //[Fact]
        public void CreateSpotsFieldL()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field L"
            };
            for (var ct = 1; ct <= 12; ct += 1)
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
        public void CreateSpotsFieldK()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field K"
            };
            for (var ct = 1; ct <= 11; ct += 1)
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
        public void CreateSpotsFieldJ()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field J"
            };
            for (var ct = 1; ct <= 11; ct += 1)
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
        public void CreateSpotsFieldI()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field I"
            };
            for (var ct = 1; ct <= 11; ct += 1)
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
        public void CreateSpotsFieldH()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field H"
            };
            for (var ct = 1; ct <= 6; ct += 1)
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
        public void CreateSpotsFieldG()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field G"
            };
            for (var ct = 1; ct <= 8; ct += 1)
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
        public void CreateSpotsFieldF()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field F"
            };
            for (var ct = 1; ct <= 8; ct += 1)
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
        public void CreateSpotsFieldE()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field E"
            };
            for (var ct = 1; ct <= 8; ct += 1)
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
        public void CreateSpotsFieldD()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field D"
            };
            for (var ct = 1; ct <= 8; ct += 1)
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
        public void CreateSpotsFieldC()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field C"
            };
            for (var ct = 1; ct <= 8; ct += 1)
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
        public void CreateSpotsFieldB()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field B"
            };
            for (var ct = 1; ct <= 8; ct += 1)
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
        public void CreateSpotsFieldA()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Field A"
            };
            for (var ct = 1; ct <= 8; ct += 1)
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
using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode.PointOfSale
{
    public class CreateSpotTable
    {
        private ITestOutputHelper Output { get; }

        public CreateSpotTable(ITestOutputHelper output)
        {
            Output = output;
        }

        //[Fact]
        public void CreateWalkway()
        {
            Output.WriteLine(Guid.NewGuid().ToString()); // 045027c8-1cb6-4d16-b2db-3b607c99f2cf
        }

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
