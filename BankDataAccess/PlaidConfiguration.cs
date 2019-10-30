using System;

namespace BankDataAccess
{
    public class PlaidConfiguration
    {
        public const string DEV_URL = "https://development.plaid.com";
        public static string DEV_CLIENT_ID => Environment.GetEnvironmentVariable("PLAID_DEV_CLIENT_ID");
        public static string DEV_PUBLIC_KEY => Environment.GetEnvironmentVariable("PLAID_DEV_PUBLIC_KEY");
        public static string DEV_SECRET => Environment.GetEnvironmentVariable("PLAID_DEV_SECRET");
        /// <summary>
        /// Access token doesn't expire, but only for one bank account and could become invalidated.
        /// </summary>
        public static string DEV_ACCESS_TOKEN_PERSONAL_CHECKING => Environment.GetEnvironmentVariable("PLAID_DEV_PERSONAL_CHECKING_TOKEN");
    }
}
