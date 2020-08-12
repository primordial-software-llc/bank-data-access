using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;

namespace FinanceApi.RequestModels
{
    public class CustomerPaymentSettingsModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("quickBooksOnlineId")]
        public int? QuickBooksOnlineId { get; set; }

        [JsonProperty("paymentFrequency")]
        public string PaymentFrequency { get; set; }

        [JsonProperty("rentPrice")]
        public decimal? RentPrice { get; set; }

        [JsonProperty("memo")]
        public string Memo { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("spots")]
        public List<Spot> Spots { get; set; }
    }
}
