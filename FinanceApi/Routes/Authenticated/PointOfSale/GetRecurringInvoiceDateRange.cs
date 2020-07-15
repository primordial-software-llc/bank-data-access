using System;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json;
using PropertyRentalManagement;
using PropertyRentalManagement.BusinessLogic;

namespace FinanceApi.Routes.Authenticated.PointOfSale
{
    public class GetRecurringInvoiceDateRange : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/recurring-invoice-date-range";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var year = int.Parse(request.QueryStringParameters["year"]);
            var month = int.Parse(request.QueryStringParameters["month"]);
            var dayOfMonth = int.Parse(request.QueryStringParameters["dayOfMonth"]);
            var frequency = (RecurringInvoices.Frequency) Enum.Parse(typeof (RecurringInvoices.Frequency), request.QueryStringParameters["frequency"], true);
            var date = new DateTime(year, month, dayOfMonth, 0, 0, 0, DateTimeKind.Utc);
            DateRange dateRange = frequency == RecurringInvoices.Frequency.Weekly
                ? RecurringInvoices.GetWeekDateRange(date)
                : RecurringInvoices.GetMonthDateRange(date);
            response.Body = JsonConvert.SerializeObject(dateRange);
        }
    }
}
