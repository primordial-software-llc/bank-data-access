using System;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    class PointOfSaleAuthorization
    {
        public bool IsAuthorized(string email)
        {
            return string.Equals(email, "timg456789@yahoo.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "timothy@primordial-software.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "kiara@primordial-software.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "kmanrique506@hotmail.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "lakeland.mipueblo@outlook.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "cvillavicencio921@gmail.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "taniagkocher@gmail.com", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsAuthorizedForReports(string email)
        {
            return string.Equals(email, "timg456789@yahoo.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "timothy@primordial-software.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "kiara@primordial-software.com", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(email, "kmanrique506@hotmail.com", StringComparison.OrdinalIgnoreCase);
        }
    }
}
