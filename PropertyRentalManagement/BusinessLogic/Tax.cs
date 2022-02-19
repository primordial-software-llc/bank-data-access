using System.Linq;
using PropertyRentalManagement.QuickBooksOnline;
using PropertyRentalManagement.QuickBooksOnline.Models.Tax;

namespace PropertyRentalManagement.BusinessLogic
{
    public class Tax
    {
        public decimal GetTaxRate(QuickBooksOnlineClient client, string taxCodeId)
        {
            var taxCode = client.Query<TaxCode>($"select * from TaxCode where Id = '{taxCodeId}'").Single();
            var taxRateId = taxCode.SalesTaxRateList.TaxRateDetail.Single().TaxRateRef.Value;
            var taxRateEntity = client.Query<TaxRate>($"select * from TaxRate where Id = '{taxRateId}'").Single();
            var taxRate = taxRateEntity.RateValue.GetValueOrDefault() / 100;
            return taxRate;
        }
    }
}
