
namespace Accounting
{
    public interface IJournalEntry
    {
        string Id { get; set; }

        int? AccountingId { get; set; }

        string Type { get; set; }

        int? Account { get; set; }

        decimal? Amount { get; set; }

        string Date { get; set; }

        string Memo { get; set; }
    }
}
