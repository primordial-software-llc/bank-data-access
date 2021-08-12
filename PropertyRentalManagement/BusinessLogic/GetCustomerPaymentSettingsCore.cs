using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using AwsDataAccess;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using PropertyRentalManagement.RequestModels;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace PropertyRentalManagement.BusinessLogic
{
    public class GetCustomerPaymentSettingsCore
    {
        private QuickBooksOnlineClient QuickBooksClient { get; }
        private DatabaseClient<Vendor> VendorClient { get; }
        private DatabaseClient<VendorLocation> VendorLocationClient { get; }
        private VendorService VendorService { get; }

        public GetCustomerPaymentSettingsCore(
            QuickBooksOnlineClient quickBooksClient,
            DatabaseClient<Vendor> vendorClient,
            DatabaseClient<VendorLocation> vendorLocationClient,
            VendorService vendorService
            )
        {
            QuickBooksClient = quickBooksClient;
            VendorClient = vendorClient;
            VendorLocationClient = vendorLocationClient;
            VendorService = vendorService;
        }

        /// <remarks>
        /// Might want to think about building out async through the entire client now that I am using parallelism.
        /// But, all this need for parallelism goes away if I build a search service.
        /// </remarks>
        public List<CustomerPaymentSettingsModel> GetCustomerPaymentSettings(
            bool includeInactiveCustomersWithSpots,
            string locationId)
        {
            Dictionary<int?, Customer> customers = null;
            List<Vendor> allVendors = null;
            List<VendorLocation> vendorLocations = null;
            Parallel.Invoke(
                () =>
                {
                    customers = QuickBooksClient.QueryAll<Customer>("select * from customer")
                        .Where(x => !Constants.NonRentalCustomerIds.Contains(x.Id.GetValueOrDefault()))
                        .ToDictionary(x => x.Id);
                },
                () =>
                {
                    allVendors = VendorClient.ScanAll(new ScanRequest(new Vendor().GetTable()));
                },
                () =>
                {
                    vendorLocations = VendorLocationClient.ScanAll(new ScanRequest(new VendorLocation().GetTable()));
                }
            );

            Dictionary<int?, Vendor> vendors = allVendors.Where(x =>
                    customers.ContainsKey(x.QuickBooksOnlineId) ||
                    (includeInactiveCustomersWithSpots && x.Spots != null && x.Spots.Any())
                )
                .ToDictionary(x => x.QuickBooksOnlineId);

            var vendorsForLocation = vendors
                .Values
                .Where(x =>
                    vendorLocations.Any(y =>
                        string.Equals(y.LocationId, locationId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.Id, y.VendorId, StringComparison.OrdinalIgnoreCase)
                    )
                )
                .ToDictionary(x => x.QuickBooksOnlineId);

            var defaultLocationId = Constants.LOCATION_ID_LAKELAND;
            foreach (var customer in customers.Values)
            {
                if (!vendors.ContainsKey(customer.Id))
                {
                    var vendorResult = VendorService.CreateVendor(customer.Id, null, null, null, defaultLocationId);
                    vendors.Add(vendorResult.Item1.QuickBooksOnlineId, vendorResult.Item1);
                    vendorLocations.Add(vendorResult.Item2);
                }
                else if (!vendorLocations.Any(y => string.Equals(vendors[customer.Id].Id, y.VendorId, StringComparison.OrdinalIgnoreCase)))
                {
                    vendorLocations.Add(VendorService.AssignVendorLocation(vendors[customer.Id].Id, defaultLocationId));
                }
            }

            if (includeInactiveCustomersWithSpots)
            {
                var allInactiveVendors = vendors.Values.Where(x => !customers.ContainsKey(x.QuickBooksOnlineId)).ToList();
                foreach (var inactiveVendor in allInactiveVendors)
                {
                    if (!vendorLocations.Any(y => string.Equals(y.VendorId, inactiveVendor.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        vendorLocations.Add(VendorService.AssignVendorLocation(inactiveVendor.Id, defaultLocationId));
                    }
                }
                var inactiveVendorsForLocation = allInactiveVendors
                    .Where(x =>
                        vendorLocations.Any(y =>
                            string.Equals(y.LocationId, locationId, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.Id, y.VendorId, StringComparison.OrdinalIgnoreCase)
                        )
                    );

                var inactiveCustomerIds = inactiveVendorsForLocation.Select(x => $"'{x.QuickBooksOnlineId}'");
                if (inactiveCustomerIds.Any())
                {
                    var inactiveCustomers = QuickBooksClient.QueryAll<Customer>($"select * from customer where Id in ({string.Join(", ", inactiveCustomerIds)}) and Active = false");
                    inactiveCustomers.ForEach(x => customers.TryAdd(x.Id, x));
                }
            }

            var json = new List<CustomerPaymentSettingsModel>();
            foreach (var vendor in vendorsForLocation.Values)
            {
                customers.TryGetValue(vendor.QuickBooksOnlineId, out var customer);
                json.Add(new CustomerPaymentSettingsModel
                {
                    Id = vendor.Id,
                    QuickBooksOnlineId = vendor.QuickBooksOnlineId,
                    PaymentFrequency = vendor.PaymentFrequency,
                    RentPrice = vendor.RentPrice,
                    Memo = vendor.Memo,
                    FirstName = customer?.GivenName,
                    LastName = customer?.FamilyName,
                    DisplayName = customer?.DisplayName,
                    Balance = customer?.Balance,
                    Spots = vendor.Spots,
                    IsActive = (customer?.Active).GetValueOrDefault()
                });
            }

            return json;
        }
    }
}
