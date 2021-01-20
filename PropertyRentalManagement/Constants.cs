
using System.Collections.Generic;

namespace PropertyRentalManagement
{
    public class Constants
    {
        public static readonly int QUICKBOOKS_TERMS_DUE_NOW = 13;
        public static readonly int QUICKBOOKS_PRODUCT_RENT = 247;
        public static readonly string QUICKBOOKS_INVOICE_LINE_TAXABLE = "TAX";
        public static readonly int QUICKBOOKS_RENTAL_TAX_RATE = 5;
        public static readonly int QUICKBOOKS_MEMO_MAX_LENGTH = 4000;
        public static readonly int QUICKBOOKS_CUSTOMER_DISPLAY_NAME_MAX_LENGTH = 500;

        public const int CUSTOMER_PARKING_A = 1859;
        public const int CUSTOMER_PARKING_B = 1861;
        public const int CUSTOMER_BAR_A = 1862;
        public const int CUSTOMER_BAR_B = 1863;
        public const int CUSTOMER_RESTAURANT = 1864;
        public const int CUSTOMER_VIRGIN_GUADALUPE = 1899;

        public static List<int> NonRentalCustomerIds => new List<int>
        {
            CUSTOMER_PARKING_A,
            CUSTOMER_PARKING_B,
            CUSTOMER_BAR_A,
            CUSTOMER_BAR_B,
            CUSTOMER_RESTAURANT,
            CUSTOMER_VIRGIN_GUADALUPE
        };

        public static string RealmId => "9130347957983546";
    }
}
