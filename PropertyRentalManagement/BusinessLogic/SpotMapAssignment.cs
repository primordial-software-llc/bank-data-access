using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement.BusinessLogic
{
    public class SpotMapAssignment
    {

        // Can't be orphaned
        // Must be assigned
        public bool IsSpotAssignedOnMap(string spotId, List<Spot> allSpots)
        {
            return !IsOrphaned(spotId, allSpots);
        }

        private bool SpotIsSectionParent(string spotId)
        {
            return GetSectionParentIds().Any(x => string.Equals(x, spotId, StringComparison.OrdinalIgnoreCase));
        }

        private Spot GetParent(string spotId, IEnumerable<Spot> allSpots)
        {
            return allSpots.FirstOrDefault(x =>
                string.Equals(spotId, x.Right, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(spotId, x.Bottom, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsOrphaned(string spotId, IList<Spot> allSpots)
        {
            var isSectionParent = SpotIsSectionParent(spotId);
            if (isSectionParent)
            {
                return false;
            }
            var parent = GetParent(spotId, allSpots);
            if (parent == null)
            {
                return true;
            }
            return IsOrphaned(parent.Id, allSpots);
        }

        public List<string> GetSectionParentIds()
        {
            return new List<string>
            {
                "ad0b0efe-52fe-43f6-b2ac-a2c2b3537440", // 79
                "50ff2d87-2a87-4934-a20b-cd4c668c85c9", // 119
                "dc30099b-e56d-4b16-b691-77c01fcb54d3", // 38
                "916250b8-505c-4fae-87a8-e119cdc46826", // 159
                "a015cb31-e6b3-449f-8be9-fd0a872cc32f", // 199
                "51e6c980-81e4-4f7a-804d-a414875a82a4", // 239
                "7cbad948-f2f1-4930-8dd6-f930f7df4518", // building 7
                "3d737dc0-6675-4530-9018-1e2db6a73774", // 301 building 8
                "39885837-2841-417f-ba82-9b2c0db70458", // Rear sheds s24
                "4b0da4b7-e066-4dbe-bdb0-4b143ebf0991", // Field S
                "5133d373-ce65-88c6-bf6f-d1999acbe338", // Field A North Walkway
                "374ff735-0380-167d-775b-77e6a4b824a1", // Field B North Walkway
                "f269ac86-9d52-d72f-dc14-c53bd6a03abe", // Field C North Walkway
                "ab4c4446-0f34-8496-6410-3d8ef38371de", // Field D North Walkway
                "02ab4f7a-3fb8-eb8d-a64f-27d08df5ead9", // Field E North Walkway
                "e71c2187-22f7-2297-908a-051a29f98d09", // Field F North Walkway
                "03263698-e1c8-5dcc-0ce7-d2354a83677e", // Field G North Walkway
                "4a7822d7-891b-4cd7-93c6-91ef6d8f9e36", // Field H North Walkway
                "9365d417-430e-447f-811c-37e8e64a3b4e", // Field I Parking
                "8edfa4f7-734c-4c3f-97b7-77d142ca60f8", // Field J Parking
                "2c838bb8-21ea-4b68-940f-3ea4da068617", // Field K Parking
                "e920ac13-8314-48e4-9609-e9f1a9cf6407", // Field L Parking
                "a8f4d1d7-a3c4-4def-b969-06a544ec3424" // South walkway South walkway
            };
        }

    }
}
