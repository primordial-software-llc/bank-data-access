using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyRentalManagement.DatabaseModel;

namespace PropertyRentalManagement
{
    public class Receipt
    {
        [JsonProperty("rentalDate")]
        public string RentalDate { get; set; }

        [JsonProperty("customer")]
        public Reference Customer { get; set; }
        
        [JsonProperty("amountOfAccount")]
        public decimal? AmountOfAccount { get; set; }
        
        [JsonProperty("rentalAmount")]
        public decimal? RentalAmount { get; set; }
        
        [JsonProperty("thisPayment")]
        public decimal? ThisPayment { get; set; }
        
        [JsonProperty("memo")]
        public string Memo { get; set; }

        [JsonProperty("spots")]
        public List<Spot> Spots { get; set; }
    }
}
