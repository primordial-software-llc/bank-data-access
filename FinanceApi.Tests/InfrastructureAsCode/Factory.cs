using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using AwsDataAccess;
using PropertyRentalManagement.DatabaseModel;
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

        /// <summary>
        /// CreateAmazonDynamoDbClient for Mi Pueblo
        /// </summary>
        /// <returns></returns>
        public static IAmazonDynamoDB CreateAmazonDynamoDbClient()
        {
            return new AmazonDynamoDBClient(CreateCredentialsForLakelandMiPuebloProfile(), HomeRegion);
        }

        public static IAmazonDynamoDB CreateAmazonDynamoDbClientForBanking()
        {
            return new AmazonDynamoDBClient(CreateCredentialsFromProfile(), HomeRegion);
        }

        /// <summary>
        /// CreateQuickBooksOnlineClient for Mi Pueblo
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static QuickBooksOnlineClient CreateQuickBooksOnlineClient(ILogging logger)
        {
            return CreateQuickBooksOnlineClient(PrivateAccounting.Constants.LakelandMiPuebloRealmId, logger);
        }

        public static QuickBooksOnlineClient CreatePrimordialQuickBooksOnlineClient(ILogging logger)
        {
            return CreateQuickBooksOnlineClient(PrivateAccounting.Constants.PrimordialSoftwareRealmId, logger);
        }

        public static QuickBooksOnlineClient CreateQuickBooksOnlineClient(string realmId, ILogging logger)
        {
            var databaseClient = new DatabaseClient<QuickBooksOnlineConnection>(CreateAmazonDynamoDbClient(), logger);
            return new QuickBooksOnlineClient(realmId, databaseClient, logger);
        }

    }
}
