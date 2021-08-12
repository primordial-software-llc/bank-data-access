using AwsDataAccess;
using FinanceApi.Tests.InfrastructureAsCode;
using PropertyRentalManagement;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.PropertyRentalManagementTests
{
    public class GetCustomerPaymentSettingsTests
    {
        private ITestOutputHelper Output { get; }

        public GetCustomerPaymentSettingsTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Test()
        {
            var logger = new XUnitLogger(Output);
            var dbClient = Factory.CreateAmazonDynamoDbClient();
            var vendorClient = new DatabaseClient<Vendor>(dbClient, logger);
            var vendorLocationClient = new DatabaseClient<VendorLocation>(dbClient, logger);
            var service = new GetCustomerPaymentSettingsCore(
                Factory.CreateQuickBooksOnlineClient(logger),
                vendorClient,
                vendorLocationClient,
                new VendorService(dbClient, logger)
            );
            service.GetCustomerPaymentSettings(true, Constants.LOCATION_ID_LAKELAND);
        }
    }
}
