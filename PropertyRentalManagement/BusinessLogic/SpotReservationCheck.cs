using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwsDataAccess;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.QuickBooksOnline.Models;
using Vendor = PropertyRentalManagement.DatabaseModel.Vendor;

namespace PropertyRentalManagement.BusinessLogic
{
    public class SpotReservationCheck : ISpotReservationCheck
    {
        private DatabaseClient<Spot> SpotDbClient { get; }
        private DatabaseClient<SpotReservation> SpotReservationDbClient { get; }
        private Dictionary<int?, Customer> AllActiveCustomers { get; }
        private List<Vendor> AllActiveVendors { get; }

        public SpotReservationCheck(
            DatabaseClient<Spot> spotDbClient,
            DatabaseClient<SpotReservation> spotReservationDbClient,
            List<Vendor> allActiveVendors,
            Dictionary<int?, Customer> allActiveCustomers)
        {
            SpotDbClient = spotDbClient;
            SpotReservationDbClient = spotReservationDbClient;
            AllActiveCustomers = allActiveCustomers;
            AllActiveVendors = allActiveVendors;
        }

        public Vendor GetVendorWhoReservedSpot(string spotId, string ignoredVendorId = null)
        {
            return AllActiveVendors.FirstOrDefault(x =>
                x.Spots != null &&
                x.Spots.Any() &&
                !string.Equals(x.Id, ignoredVendorId, StringComparison.OrdinalIgnoreCase) &&
                x.Spots.Any(vendorSpot => vendorSpot.Id == spotId));
        }

        public Tuple<Vendor, SpotReservation> GetReservation(string spotId, string rentalDate, string ignoredVendorId = null)
        {
            var vendorReserved = GetVendorWhoReservedSpot(spotId, ignoredVendorId);
            if (vendorReserved != null)
            {
                return new Tuple<Vendor, SpotReservation>(vendorReserved, null);
            }
            var oneTimeReservation = SpotReservationDbClient.Get(new SpotReservation
            {
                SpotId = spotId,
                RentalDate = rentalDate
            },
            true).Result;
            if (oneTimeReservation != null)
            {
                return new Tuple<Vendor, SpotReservation>(null, oneTimeReservation);
            }
            return null;
        }

        public async Task<List<string>> GetSpotConflicts(List<Spot> unAuthoritativeSpots, string rentalDate, string ignoredVendorId = null)
        {
            var conflicts = new List<string>();
            if (unAuthoritativeSpots == null || !unAuthoritativeSpots.Any())
            {
                return conflicts;
            }

            var authoritativeSpots = await SpotDbClient.Get(unAuthoritativeSpots);

            foreach (var spot in authoritativeSpots)
            {
                if (spot.Restricted.GetValueOrDefault())
                {
                    conflicts.Add($"Spot {spot.Section?.Name} - {spot.Name} is restricted from reservation.");
                    continue;
                }
                var reservation = GetReservation(spot.Id, rentalDate, ignoredVendorId);
                if (reservation?.Item1 != null)
                {
                    var conflictDescription = $"Spot {spot.Section?.Name} - {spot.Name} is reserved indefinitely";
                    if (reservation.Item1.QuickBooksOnlineId > 0)
                    {
                        if (AllActiveCustomers.TryGetValue(reservation.Item1.QuickBooksOnlineId, out var customer))
                        {
                            conflictDescription += $" by {customer.DisplayName}";
                        }
                    }
                    conflicts.Add(conflictDescription);
                }
                if (reservation?.Item2 != null)
                {
                    var conflictDescription = $"Spot {spot.Section?.Name} - {spot.Name} is reserved on {rentalDate}";
                    if (reservation.Item2.QuickBooksOnlineId > 0)
                    {
                        if (AllActiveCustomers.TryGetValue(reservation.Item2.QuickBooksOnlineId, out var customer))
                        {
                            conflictDescription += $" by {customer.DisplayName}";
                        }
                    }
                    conflicts.Add(conflictDescription);
                }
            }
            return conflicts;
        }
    }
}
