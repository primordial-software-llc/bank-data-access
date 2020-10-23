using System;
using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public class MetaData
    {
        [JsonProperty("CreateTime")]
        public DateTimeOffset CreateTime { get; set; }

        [JsonProperty("LastUpdatedTime")]
        public DateTimeOffset LastUpdatedTime { get; set; }
    }
}
