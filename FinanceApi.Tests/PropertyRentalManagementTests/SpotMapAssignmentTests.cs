using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using AwsDataAccess;
using FinanceApi.Tests.InfrastructureAsCode;
using PropertyRentalManagement.BusinessLogic;
using PropertyRentalManagement.DatabaseModel;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests.PropertyRentalManagementTests
{
    public class SpotMapAssignmentTests
    {
        private ITestOutputHelper Output { get; }
        private List<Spot> AllSpots { get; }
        private List<Spot> ParentedSpots { get; }
        private List<Spot> OrphanedSpots { get; }

        public readonly string NewSpotBottomRightBottom6Id = Guid.NewGuid().ToString();
        public readonly string OrphanedSpotBottomRightBottom6Id = Guid.NewGuid().ToString();

        public SpotMapAssignmentTests(ITestOutputHelper output)
        {
            Output = output;
            AllSpots = new List<Spot>();
            ParentedSpots = new List<Spot>();
            OrphanedSpots = new List<Spot>();

            foreach (var spotId in new SpotMapAssignment().GetSectionParentIds())
            {
                AllSpots.Add(new Spot
                {
                    Id = spotId
                });
            }
            
            var newSpotRight1 = new Spot { Id = Guid.NewGuid().ToString() };
            var newSpotRight2 = new Spot { Id = Guid.NewGuid().ToString() };
            var newSpotRight3 = new Spot { Id = Guid.NewGuid().ToString() };
            var newSpotBottom1 = new Spot { Id = Guid.NewGuid().ToString() };
            var newSpotBottom2 = new Spot { Id = Guid.NewGuid().ToString() };
            var newSpotBottom3 = new Spot { Id = Guid.NewGuid().ToString() };
            var newSpotBottomRight4 = new Spot { Id = Guid.NewGuid().ToString() };
            var newSpotBottomRight5 = new Spot { Id = Guid.NewGuid().ToString() };
            var newSpotBottomRightBottom6 = new Spot { Id = NewSpotBottomRightBottom6Id };

            var originSpot = AllSpots.First();
            originSpot.Right = newSpotRight1.Id;
            newSpotRight1.Right = newSpotRight2.Id;
            newSpotRight2.Right = newSpotRight3.Id;
            originSpot.Bottom = newSpotBottom1.Id;
            newSpotBottom1.Bottom = newSpotBottom2.Id;
            newSpotBottom2.Bottom = newSpotBottom3.Id;
            newSpotBottom3.Right = newSpotBottomRight4.Id;
            newSpotBottomRight4.Right = newSpotBottomRight5.Id;
            newSpotBottomRight5.Bottom = newSpotBottomRightBottom6.Id;

            ParentedSpots.Add(newSpotRight1);
            ParentedSpots.Add(newSpotRight2);
            ParentedSpots.Add(newSpotRight3);
            ParentedSpots.Add(newSpotBottom1);
            ParentedSpots.Add(newSpotBottom2);
            ParentedSpots.Add(newSpotBottom3);
            ParentedSpots.Add(newSpotBottomRight4);
            ParentedSpots.Add(newSpotBottomRight5);
            ParentedSpots.Add(newSpotBottomRightBottom6);

            var orphanedParentSpot = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotRight1 = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotRight2 = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotRight3 = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotBottom1 = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotBottom2 = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotBottom3 = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotBottomRight4 = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotBottomRight5 = new Spot { Id = Guid.NewGuid().ToString() };
            var orphanedSpotBottomRightBottom6 = new Spot { Id = OrphanedSpotBottomRightBottom6Id };

            orphanedParentSpot.Right = orphanedSpotRight1.Id;
            orphanedSpotRight1.Right = orphanedSpotRight2.Id;
            orphanedSpotRight2.Right = orphanedSpotRight3.Id;
            orphanedParentSpot.Bottom = orphanedSpotBottom1.Id;
            orphanedSpotBottom1.Bottom = orphanedSpotBottom2.Id;
            orphanedSpotBottom2.Bottom = orphanedSpotBottom3.Id;
            orphanedSpotBottom3.Right = orphanedSpotBottomRight4.Id;
            orphanedSpotBottomRight4.Right = orphanedSpotBottomRight5.Id;
            orphanedSpotBottomRight5.Bottom = orphanedSpotBottomRightBottom6.Id;

            OrphanedSpots.Add(orphanedParentSpot);
            OrphanedSpots.Add(orphanedSpotRight1);
            OrphanedSpots.Add(orphanedSpotRight2);
            OrphanedSpots.Add(orphanedSpotRight3);
            OrphanedSpots.Add(orphanedSpotBottom1);
            OrphanedSpots.Add(orphanedSpotBottom2);
            OrphanedSpots.Add(orphanedSpotBottom3);
            OrphanedSpots.Add(orphanedSpotBottomRight4);
            OrphanedSpots.Add(orphanedSpotBottomRight5);
            OrphanedSpots.Add(orphanedSpotBottomRightBottom6);

            AllSpots.AddRange(ParentedSpots);
            AllSpots.AddRange(OrphanedSpots);
        }

        [Fact]
        public void UnknownSpotIsNotAssigned()
        {
            var spotMapAssignment = new SpotMapAssignment();
            Assert.False(spotMapAssignment.IsSpotAssignedOnMap(Guid.NewGuid().ToString(), AllSpots));
        }

        [Fact]
        public void SectionParentsAreAssigned()
        {
            var spotMapAssignment = new SpotMapAssignment();
            Assert.True(spotMapAssignment.GetSectionParentIds().All(x => spotMapAssignment.IsSpotAssignedOnMap(x, AllSpots)));
        }

        [Fact]
        public void SpotWithAssignedParentIsAssigned()
        {
            var spotMapAssignment = new SpotMapAssignment();
            foreach (var parentedSpot in ParentedSpots)
            {
                Assert.True(spotMapAssignment.IsSpotAssignedOnMap(parentedSpot.Id, AllSpots));
            }
        }

        [Fact]
        public void SpotWithUnassignedParentIsNotAssigned()
        {
            var spotMapAssignment = new SpotMapAssignment();
            foreach (var orphanedSpot in OrphanedSpots)
            {
                Assert.False(spotMapAssignment.IsSpotAssignedOnMap(orphanedSpot.Id, AllSpots));
            }
        }

        [Fact]
        public void PrintOrphanedSpots()
        {
            var spotClient = new DatabaseClient<Spot>(Factory.CreateAmazonDynamoDbClient(), new XUnitLogger(Output));
            var spots = spotClient.ScanAll(new ScanRequest(new Spot().GetTable()))
                .OrderBy(x => x.Section?.Name)
                .ThenBy(x => x.Name)
                .ToList();

            var spotMapAssignment = new SpotMapAssignment();
            foreach (var spot in spots)
            {
                if (!spotMapAssignment.IsSpotAssignedOnMap(spot.Id, spots))
                {
                    Output.WriteLine($"{spot.Id} - {spot.Section.Name} - {spot.Name}");
                }
            }
        }
        
    }
}
