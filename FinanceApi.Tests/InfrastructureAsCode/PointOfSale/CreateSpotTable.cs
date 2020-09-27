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
        public void CreateSouthWalkwaySpots()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway"
            };

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Office",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Office",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Office",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mobile Home",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mobile Home",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mobile Home",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Red Barn",
                Section = section
            });
        }

        //[Fact]
        public void CreateSpotsFieldL()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = "3408b26a-b7ed-4e76-8a42-9b574181afae",
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "3408b26a-b7ed-4e76-8a42-9b574181afae",
                    Name = "Field L"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "3408b26a-b7ed-4e76-8a42-9b574181afae",
                    Name = "Field L"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "3408b26a-b7ed-4e76-8a42-9b574181afae",
                    Name = "Field L"
                }
            });
        }

        //[Fact]
        public void CreateSpotsFieldK()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = "e8651c71-e5dd-4706-a18c-d2ee0e0da00c",
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "e8651c71-e5dd-4706-a18c-d2ee0e0da00c",
                    Name = "Field K"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "e8651c71-e5dd-4706-a18c-d2ee0e0da00c",
                    Name = "Field K"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "e8651c71-e5dd-4706-a18c-d2ee0e0da00c",
                    Name = "Field K"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "e8651c71-e5dd-4706-a18c-d2ee0e0da00c",
                    Name = "Field K"
                }
            });
        }

        //[Fact]
        public void CreateSpotsFieldJ()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = "596c8ac6-ebf9-4438-9973-4a516288d7b9",
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "596c8ac6-ebf9-4438-9973-4a516288d7b9",
                    Name = "Field J"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "596c8ac6-ebf9-4438-9973-4a516288d7b9",
                    Name = "Field J"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "596c8ac6-ebf9-4438-9973-4a516288d7b9",
                    Name = "Field J"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "596c8ac6-ebf9-4438-9973-4a516288d7b9",
                    Name = "Field J"
                }
            });
        }

        //[Fact]
        public void CreateSpotsFieldI()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = "2d2fa812-3bcb-4955-9a7c-63922e7392fa",
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "2d2fa812-3bcb-4955-9a7c-63922e7392fa",
                    Name = "Field I"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "2d2fa812-3bcb-4955-9a7c-63922e7392fa",
                    Name = "Field I"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "2d2fa812-3bcb-4955-9a7c-63922e7392fa",
                    Name = "Field I"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Parking",
                Section = new Section
                {
                    Id = "2d2fa812-3bcb-4955-9a7c-63922e7392fa",
                    Name = "Field I"
                }
            });
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "47afac0b-67c2-4807-a88d-3ea9e1775661",
                    Name = "Field H"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "7 - North Walkway",
                Section = new Section
                {
                    Id = "47afac0b-67c2-4807-a88d-3ea9e1775661",
                    Name = "Field H"
                }
            });
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "8 - North Walkway",
                Section = new Section
                {
                    Id = "47afac0b-67c2-4807-a88d-3ea9e1775661",
                    Name = "Field H"
                }
            });
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "acddb4d0-1983-46b4-8d3c-e390f070bf0c",
                    Name = "Field G"
                }
            });
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "cd7587ed-e3f5-4e81-b512-881a67f57ab8",
                    Name = "Field F"
                }
            });
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "469c1c7c-f2f9-4c64-9857-8567821b8adf",
                    Name = "Field E"
                }
            });
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "dc55ee40-e618-4016-9ec3-b003d8e8fe52",
                    Name = "Field D"
                }
            });
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "2f62d887-39f4-44b7-92a2-4fd9bbecd423",
                    Name = "Field C"
                }
            });
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "3c27b448-514c-4d4b-bdd5-def1414e8d3b",
                    Name = "Field B"
                }
            });
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
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "South Walkway",
                Section = new Section
                {
                    Id = "96be73ae-cd0e-49ce-8dd2-ccf02fba1c30",
                    Name = "Field A"
                }
            });
        }

        //[Fact]
        public void CreateSpotsRearShed()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = "734541c3-e863-4b4f-9dd6-2bcf606a691d",
                Name = "Rear Sheds"
            };
            
            for (var ct = 1; ct <= 24; ct += 1)
            {
                dbClient.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "S" + ct,
                    Section = section
                });
            }
            for (var ct = 1; ct <= 4; ct += 1)
            {
                dbClient.Create(new Spot
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Bathrooms",
                    Section = section
                });
            }

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "White Building",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Yellow Building",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "S11 - 1",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "S11 - 2",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "S11 - 3",
                Section = section
            });
            
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "S11 - 4",
                Section = section
            });
            
            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "S9 - 2",
                Section = section
            });

            dbClient.Create(new Spot
            {
                Id = Guid.NewGuid().ToString(),
                Name = "S12 - 2",
                Section = section
            });
        }

        //[Fact]
        public void CreateSpotsBuilding8()
        {
            var client = Factory.CreateAmazonDynamoDbClient();
            var dbClient = new DatabaseClient<Spot>(client);
            var section = new Section
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Building 8"
            };
            for (var ct = 279; ct <= 301; ct += 1)
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
