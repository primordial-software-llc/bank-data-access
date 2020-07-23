
namespace Accounting
{
    public interface IAccountingClient
    {
        int? RecordExpense(IJournalEntry journalEntry);
        int? RecordIncome(IJournalEntry journalEntry);
    }
}
