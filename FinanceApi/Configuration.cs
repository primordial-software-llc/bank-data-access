using System;

namespace FinanceApi
{
    public class Configuration
    {
        public static string PlaidUrl => Environment.GetEnvironmentVariable("PLAID_URL");
        public static string PlaidClientId => Environment.GetEnvironmentVariable("PLAID_CLIENT_ID");
        public static string PlaidPublicKey => Environment.GetEnvironmentVariable("PLAID_PUBLIC_KEY");
        public static string PlaidSecret => Environment.GetEnvironmentVariable("PLAID_SECRET");
        public static string FinanceApiCognitoUserPoolId => Environment.GetEnvironmentVariable("FINANCE_API_COGNITO_USER_POOL_ID");
        public static string FinanceApiCognitoClientId => Environment.GetEnvironmentVariable("FINANCE_API_COGNITO_CLIENT_ID");
        public static string StripeApiSecretKey => Environment.GetEnvironmentVariable("STRIPE_API_SECRET_KEY");
        public static string StripeIncomeCalculatorProductPlanId = Environment.GetEnvironmentVariable("STRIPE_INCOME_CALCULATOR_PRODUCT_PLAN_ID");
        public static string DwollaKey = Environment.GetEnvironmentVariable("DWOLLA_KEY");
        public static string DwollaSecret = Environment.GetEnvironmentVariable("DWOLLA_SECRET");
        public static string DwollaUrl = Environment.GetEnvironmentVariable("DWOLLA_URL");
    }
}
