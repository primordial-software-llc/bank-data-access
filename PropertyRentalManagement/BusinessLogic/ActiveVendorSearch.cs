using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline.Models;

namespace PropertyRentalManagement.BusinessLogic
{
    public class ActiveVendorSearch
    {
        public Dictionary<int?, DatabaseModel.Vendor> GetActiveVendors(Dictionary<int?, Customer> allActiveCustomers, VendorService vendorService, RecurringInvoices.Frequency paymentFrequency)
        {
            var activeVendors = vendorService.GetByPaymentFrequency(paymentFrequency)
                .Where(x => allActiveCustomers.ContainsKey(x.QuickBooksOnlineId))
                .ToDictionary(x => x.QuickBooksOnlineId);
            return activeVendors;
        }

        public List<DatabaseModel.Vendor> GetActiveVendors(
            Dictionary<int?, Customer> allActiveCustomers,
            DatabaseClient<DatabaseModel.Vendor> vendorDbClient)
        {
            var activeVendors = vendorDbClient.ScanAll(new ScanRequest(new DatabaseModel.Vendor().GetTable()))
                .Where(x => allActiveCustomers.ContainsKey(x.QuickBooksOnlineId))
                .ToList();
            return activeVendors;
        }
    }
}
