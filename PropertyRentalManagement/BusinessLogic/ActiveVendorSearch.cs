using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ActiveVendorSearch
    {
// MIght want to consolidate these methods and inject the vendors.
        public Dictionary<int?, DatabaseModel.Vendor> GetActiveVendors(Dictionary<int?, Customer> allActiveCustomers, VendorService vendorService, RecurringInvoices.Frequency paymentFrequency)
        {
            var activeVendors = vendorService.GetByPaymentFrequency(paymentFrequency)
                .Where(x => allActiveCustomers.ContainsKey(x.QuickBooksOnlineId))
                .ToDictionary(x => x.QuickBooksOnlineId);
            return activeVendors;
        }

        public List<DatabaseModel.Vendor> GetActiveVendors(QuickBooksOnlineClient quickBooksClient, DatabaseClient<DatabaseModel.Vendor> vendorDbClient)
        {
            var allActiveCustomers = quickBooksClient
                .QueryAll<Customer>("select * from customer")
                .ToDictionary(x => x.Id);
            var activeVendors = vendorDbClient.GetAll()
                .Where(x => allActiveCustomers.ContainsKey(x.QuickBooksOnlineId))
                .ToList();
            return activeVendors;
        }
    }
}
