using Newtonsoft.Json;

namespace PropertyRentalManagement
{
    public class CalendarDay
    {
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("dayOfMonth")]
        public int DayOfMonth { get; set; }
    }
}
