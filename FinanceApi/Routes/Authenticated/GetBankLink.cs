using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated
{
    class GetBankLink : IRoute
    {
        public string HttpMethod => "GET";
        public string Path => "/bank-link";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var itemJson = new JArray();
            var institutionsJson = new JArray();
            var institutions = new HashSet<string>();
            var bankClient = new BankAccessClient(Configuration.PlaidUrl, new Logger());
            foreach (var bankLink in user.BankLinks ?? new List<BankLink>())
            {
                var item = bankClient.GetItem(bankLink.AccessToken)["item"];
                institutions.Add(item["institution_id"].Value<string>());
                itemJson.Add(item);
            }
            foreach (var institution in institutions)
            {
                institutionsJson.Add(bankClient.GetInstitution(institution)["institution"]);
            }
            foreach (var item in itemJson)
            {
                var institutionId = item["institution_id"].Value<string>();
                item["institution"] = institutionsJson.First(x =>
                    string.Equals(x["institution_id"].Value<string>(), institutionId, StringComparison.OrdinalIgnoreCase));
            }
            response.Body = itemJson.ToString();
        }
    }
}
