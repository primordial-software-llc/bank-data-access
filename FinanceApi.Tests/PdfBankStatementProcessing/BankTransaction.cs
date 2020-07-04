
using System;

namespace Tests.PdfBankStatementProcessing
{
    public class BankTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}
