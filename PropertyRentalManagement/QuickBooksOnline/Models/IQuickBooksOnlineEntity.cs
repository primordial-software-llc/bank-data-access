
namespace PropertyRentalManagement.QuickBooksOnline.Models
{
    public interface IQuickBooksOnlineEntity
    {
        string SyncToken { get; set; }
        int? Id { get; set; }
        string EntityName { get; }
    }
}
