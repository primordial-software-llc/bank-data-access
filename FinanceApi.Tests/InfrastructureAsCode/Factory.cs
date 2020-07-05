using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using AwsTools;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;

namespace FinanceApi.Tests.InfrastructureAsCode
{
    public class Factory
    {
        public static RegionEndpoint HomeRegion => RegionEndpoint.USEast1;
        public static AWSCredentials CreateCredentialsForLakelandMiPuebloProfile()
        {
            var chain = new CredentialProfileStoreChain();
            var profile = "lakeland-mi-pueblo";
            if (!chain.TryGetAWSCredentials(profile, out AWSCredentials awsCredentials))
            {
                throw new Exception($"AWS credentials not found for \"{profile}\" profile.");
            }
            return awsCredentials;
        }

        public static AWSCredentials CreateCredentialsFromProfile()
        {
            var chain = new CredentialProfileStoreChain();
            var profile = "deploy-production";
            if (!chain.TryGetAWSCredentials(profile, out AWSCredentials awsCredentials))
            {
                throw new Exception($"AWS credentials not found for \"{profile}\" profile.");
            }
            return awsCredentials;
        }

        public static IAmazonDynamoDB CreateAmazonDynamoDbClient()
        {
            return new AmazonDynamoDBClient(CreateCredentialsForLakelandMiPuebloProfile(), HomeRegion);
        }

        public static QuickBooksOnlineClient CreateQuickBooksOnlineClient(ILogging logger)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(CreateAmazonDynamoDbClient());
            return new QuickBooksOnlineClient(Configuration.RealmId, databaseClient, logger);
        }
    }
}
