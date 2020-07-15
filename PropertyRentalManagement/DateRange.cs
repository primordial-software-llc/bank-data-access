using System;
using Newtonsoft.Json;

namespace PropertyRentalManagement
{
    public class DateRange
    {
        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }
    }
}
