using System;
using NodaTime;

namespace FinanceApi
{
    public class Configuration
    {
        public static string PLAID_URL => Environment.GetEnvironmentVariable("PLAID_URL");
        public static string PLAID_CLIENT_ID => Environment.GetEnvironmentVariable("PLAID_CLIENT_ID");
        public static string PLAID_PUBLIC_KEY => Environment.GetEnvironmentVariable("PLAID_PUBLIC_KEY");
        public static string PLAID_SECRET => Environment.GetEnvironmentVariable("PLAID_SECRET");
        public static string FINANCE_API_COGNITO_USER_POOL_ID => Environment.GetEnvironmentVariable("FINANCE_API_COGNITO_USER_POOL_ID");
        public static string FINANCE_API_COGNITO_CLIENT_ID => Environment.GetEnvironmentVariable("FINANCE_API_COGNITO_CLIENT_ID");
        public static string STRIPE_API_SECRET_KEY => Environment.GetEnvironmentVariable("STRIPE_API_SECRET_KEY");
        public static string STRIPE_INCOME_CALCULATOR_PRODUCT_PLAN_ID = Environment.GetEnvironmentVariable("STRIPE_INCOME_CALCULATOR_PRODUCT_PLAN_ID");

        public static string RealmId => "9130347957983546";

        public static decimal POLK_COUNTY_RENTAL_SALES_TAX_RATE = Decimal.Parse(Environment.GetEnvironmentVariable("POLK_COUNTY_RENTAL_SALES_TAX_RATE"));

        public static DateTimeZone LakeLandMiPuebloTimeZone => DateTimeZoneProviders.Tzdb["America/New_York"];

        public static BankAccessClient BankClient => new BankAccessClient(
            PLAID_URL,
            PLAID_CLIENT_ID,
            PLAID_SECRET,
            PLAID_PUBLIC_KEY,
            new ConsoleLogger());

    }
}
