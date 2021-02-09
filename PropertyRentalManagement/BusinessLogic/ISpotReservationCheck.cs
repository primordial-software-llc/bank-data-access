using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement.BusinessLogic
{
    public interface ISpotReservationCheck
    {
        Vendor GetVendorWhoReservedSpot(string spotId, string ignoredVendorId = null);
        Tuple<Vendor, SpotReservation> GetReservation(string spotId, string rentalDate, string ignoredVendorId = null);
        Task<List<string>> GetSpotConflicts(List<Spot> unAuthoritativeSpots, string rentalDate, string ignoredVendorId = null);
    }
}