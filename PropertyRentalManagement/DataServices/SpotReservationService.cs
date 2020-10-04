using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement.DataServices
{
    public class SpotReservationService
    {
        private DatabaseClient<SpotReservation> Client { get; set; }

        public SpotReservationService(DatabaseClient<SpotReservation> client)
        {
            Client = client;
        }

        public List<SpotReservation> GetSpotReservations(string rentalDate)
        {
            var queryRequest = new QueryRequest(new SpotReservation().GetTable())
            {
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":rentalDate", new AttributeValue {S = rentalDate}}
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#rentalDate", "rentalDate" }
                },
                KeyConditionExpression = "#rentalDate = :rentalDate"
            };
            return Client.QueryAll(queryRequest);
        }

        public List<SpotReservation> GetSpotReservationsByVendor(string vendorId)
        {
            var queryRequest = new QueryRequest(new SpotReservation().GetTable())
            {
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":vendorId", new AttributeValue {S = vendorId}}
                },
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#vendorId", "vendorId" }
                },
                KeyConditionExpression = "#vendorId = :vendorId",
                IndexName = "vendorId"
            };
            return Client.QueryAll(queryRequest);
        }
    }
}
