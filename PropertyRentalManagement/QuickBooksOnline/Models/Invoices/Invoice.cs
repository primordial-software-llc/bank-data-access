﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline.Models.Invoices
{
    public class Invoice : IQuickBooksOnlineEntity
    {
        [JsonIgnore]
        public string EntityName => "Invoice";

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("TxnDate")]
        public string TxnDate { get; set; }

        [JsonProperty("CustomerRef")]
        public Reference CustomerRef { get; set; }

        [JsonProperty("Line")]
        public List<InvoiceLine> Line { get; set; }

        [JsonProperty("TxnTaxDetail")]
        public TxnTaxDetail TxnTaxDetail { get; set; }

        [JsonProperty("PrivateNote")]
        public string PrivateNote { get; set; }

        [JsonProperty("SalesTermRef")]
        public Reference SalesTermRef { get; set; }
    }
}