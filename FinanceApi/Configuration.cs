using System;

namespace FinanceApi
{
    public class Configuration
    {
        public const string DEV_URL = "https://development.plaid.com";
        public static string DEV_CLIENT_ID => Environment.GetEnvironmentVariable("PLAID_DEV_CLIENT_ID");
        public static string DEV_PUBLIC_KEY => Environment.GetEnvironmentVariable("PLAID_DEV_PUBLIC_KEY");
        public static string DEV_SECRET => Environment.GetEnvironmentVariable("PLAID_DEV_SECRET");
        public static string FINANCE_API_COGNITO_USER_POOL_ID => Environment.GetEnvironmentVariable("FINANCE_API_COGNITO_USER_POOL_ID");
        public static string FINANCE_API_COGNITO_CLIENT_ID => Environment.GetEnvironmentVariable("FINANCE_API_COGNITO_CLIENT_ID");
    }
}
