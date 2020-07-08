using System;
using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class MetaData
    {
        [JsonProperty("CreateTime")]
        public DateTime CreateTime { get; set; }

        [JsonProperty("LastUpdatedTime")]
        public DateTime LastUpdatedTime { get; set; }
    }
}
