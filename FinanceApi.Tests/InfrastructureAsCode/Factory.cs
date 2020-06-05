using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using PropertyRentalManagement;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline;

namespace Tests
{
    public class Factory
    {
        public static RegionEndpoint HomeRegion => RegionEndpoint.USEast1;
        public static AWSCredentials CreateCredentialsFromProfile()
        {
            var chain = new CredentialProfileStoreChain();
            var profile = "lakeland-mi-pueblo";
            if (!chain.TryGetAWSCredentials(profile, out AWSCredentials awsCredentials))
            {
                throw new Exception($"AWS credentials not found for \"{profile}\" profile.");
            }
            return awsCredentials;
        }

        public static IAmazonDynamoDB CreateAmazonDynamoDbClient()
        {
            return new AmazonDynamoDBClient(CreateCredentialsFromProfile(), HomeRegion);
        }

        public static QuickBooksOnlineClient CreateQuickBooksOnlineClient(ILogger logger)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(CreateAmazonDynamoDbClient());
            return new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, logger);
        }
    }
}
