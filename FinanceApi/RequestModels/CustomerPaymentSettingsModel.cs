using Newtonsoft.Json;
using PropertyRentalManagement.QuickBooksOnline.Models;

namespace FinanceApi.RequestModels
{
    public class CustomerPaymentSettingsModel
    {
        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("vendor")]
        public PropertyRentalManagement.DatabaseModel.Vendor Vendor { get; set; }
    }
}
