using System.Collections.Generic;
using System.Linq;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace PropertyRentalManagement.BusinessLogic
{
    public class SpotReservationCheck
    {
        private DatabaseClient<SpotReservation> SpotReservationDbClient { get; }
        private DatabaseClient<Vendor> VendorDbClient { get; set; }
        private QuickBooksOnlineClient QuickBooksClient { get; set; }

        public SpotReservationCheck(
            DatabaseClient<SpotReservation> spotReservationDbClient,
            DatabaseClient<Vendor> vendorDbClient,
            QuickBooksOnlineClient quickBooksClient)
        {
            SpotReservationDbClient = spotReservationDbClient;
            VendorDbClient = vendorDbClient;
            QuickBooksClient = quickBooksClient;
        }

        public List<string> GetSpotConflicts(List<Spot> spots, string rentalDate)
        {
            var conflicts = new List<string>();
            if (spots == null)
            {
                return conflicts;
            }
            var allActiveVendors = new ActiveVendorSearch()
                .GetActiveVendors(QuickBooksClient, VendorDbClient)
                .Where(x => x.Spots != null && x.Spots.Any())
                .ToList();
            foreach (var spot in spots)
            {
                foreach (var vendor in allActiveVendors)
                {
                    if (vendor.Spots.Any(vendorSpot => vendorSpot.Id == spot.Id))
                    {
                        conflicts.Add(GetConflictDescription(spot, rentalDate, vendor.QuickBooksOnlineId));
                    }
                }
                var conflictingSpot = SpotReservationDbClient.Get(new SpotReservation
                {
                    SpotId = spot.Id,
                    RentalDate = rentalDate
                });
                if (conflictingSpot != null)
                {
                    conflicts.Add(GetConflictDescription(spot, rentalDate, conflictingSpot.QuickBooksOnlineCustomerId));
                }
            }
            return conflicts;
        }

        private string GetConflictDescription(Spot spot, string rentalDate, int? quickBooksOnlineCustomerId)
        {
            var conflictDescription = $"Spot {spot.Section?.Name} - {spot.Name} is already reserved for {rentalDate}";
            if (quickBooksOnlineCustomerId > 0)
            {
                var customer = QuickBooksClient.Query<Customer>($"select * from Customer where Id = '{quickBooksOnlineCustomerId}'").FirstOrDefault();
                if (customer != null)
                {
                    conflictDescription += $" by {customer.DisplayName}";
                }
            }
            return conflictDescription;
        }

    }
}
