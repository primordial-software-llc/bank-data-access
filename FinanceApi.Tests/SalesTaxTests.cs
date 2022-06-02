using System;
using System.Collections.Generic;
using System.Text;
using AwsDataAccess;
using FinanceApi.Tests.InfrastructureAsCode;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
{
    public class SalesTaxTests
    {
        private ITestOutputHelper Output { get; }

        public SalesTaxTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Tax_Rate_Id_5_Locked_To_6_Point_8_Percent()
        {
            var dbClient = Factory.CreateAmazonDynamoDbClient();
            var logger = new ConsoleLogger();
            var locationClient = new DatabaseClient<Location>(dbClient, logger);
            var quickbooksClient = Factory.CreateQuickBooksOnlineClient(logger);
            var location = locationClient.Get(new Location { Id = PropertyRentalManagement.Constants.LOCATION_ID_LAKELAND }).Result;
            var taxRate = new Tax().GetTaxRate(quickbooksClient, 5.ToString());
            Assert.Equal(.068m, taxRate);
        }

        [Fact]
        public void Tax_Rate_Id_7_Locked_To_6_Point_5_Percent()
        {
            var dbClient = Factory.CreateAmazonDynamoDbClient();
            var logger = new ConsoleLogger();
            var locationClient = new DatabaseClient<Location>(dbClient, logger);
            var quickbooksClient = Factory.CreateQuickBooksOnlineClient(logger);
            var location = locationClient.Get(new Location { Id = PropertyRentalManagement.Constants.LOCATION_ID_LAKELAND }).Result;
            var taxRate = new Tax().GetTaxRate(quickbooksClient, 7.ToString());
            Assert.Equal(.065m, taxRate);
        }

        [Fact]
        public void Lakeland_Tax_Rate_Is_6_Point_5_Percent()
        {
            var dbClient = Factory.CreateAmazonDynamoDbClient();
            var logger = new ConsoleLogger();
            var locationClient = new DatabaseClient<Location>(dbClient, logger);
            var quickbooksClient = Factory.CreateQuickBooksOnlineClient(logger);
            var location = locationClient.Get(new Location { Id = PropertyRentalManagement.Constants.LOCATION_ID_LAKELAND }).Result;
            var taxRate = new Tax().GetTaxRate(quickbooksClient, location.SalesRentalTaxRateId.ToString());
            Assert.Equal(.065m, taxRate);
        }
    }
}
