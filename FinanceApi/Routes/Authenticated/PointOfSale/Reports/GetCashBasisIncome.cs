using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using FinanceApi.ResponseModels;
using Newtonsoft.Json;
using NodaTime;
using PropertyRentalManagement.DatabaseModel;
using PropertyRentalManagement.DataServices;
using PropertyRentalManagement.QuickBooksOnline;

namespace FinanceApi.Routes.Authenticated.PointOfSale.Reports
{
    public class GetCashBasisIncome : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/point-of-sale/cash-basis-income";

        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var start = request.QueryStringParameters["start"];
            var end = request.QueryStringParameters["end"];

            var startDate = DateTime.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var easternStart = Configuration.LakeLandMiPuebloTimeZone.AtLeniently(LocalDateTime.FromDateTime(startDate));
            var easternEnd = Configuration.LakeLandMiPuebloTimeZone.AtLeniently(LocalDateTime.FromDateTime(endDate));

            var dbClient = new AmazonDynamoDBClient();
            var qboDbClient = new DatabaseClient<QuickBooksOnlineConnection>(dbClient, new ConsoleLogger());
            var qboClient = new QuickBooksOnlineClient(Configuration.RealmId, qboDbClient, new ConsoleLogger());
            var report = PropertyRentalManagement.Reports.IncomeReport.RunReport(
                easternStart.ToDateTimeOffset(),
                easternEnd.ToDateTimeOffset(),
                qboClient,
                PropertyRentalManagement.Constants.NonRentalCustomerIds);

            var income = new List<Income>();
            foreach (var item in report.Payments)
            {
                income.Add(new Income
                {
                    RecordType = "payment",
                    Customer = item.CustomerRef.Name,
                    Amount = item.TotalAmount.ToString(),
                    Date = item.MetaData.CreateTime.ToString("O")
                });
            }
            foreach (var item in report.SalesReceipts)
            {
                income.Add(new Income
                {
                    RecordType = "salesReceipt",
                    Customer = item.CustomerRef.Name,
                    Amount = item.TotalAmount.ToString(),
                    Date = item.MetaData.CreateTime.ToString("O")
                });
            }
            income = income.OrderBy(x => x.Date).ToList();
            response.Body = JsonConvert.SerializeObject(income);
        }
    }
}
