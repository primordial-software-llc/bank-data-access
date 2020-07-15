using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PropertyRentalManagement;
using PropertyRentalManagement.BusinessLogic;

namespace FinanceApi.RequestModels
{
    public class GetRecurringInvoiceDateRangeModel
    {
        [JsonProperty("calendarDay")]
        public CalendarDay CalendarDay { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("frequency")]
        public RecurringInvoices.Frequency Frequency { get; set;}
    }
}
