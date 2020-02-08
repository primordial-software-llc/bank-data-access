using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApi.Routes.Authenticated;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
{
    public class Test
    {
        private readonly ITestOutputHelper output;

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Stripe_Purchase()
        {
            var subscription = new PostPurchase().Purchase(
                "timg456789@yahoo.com",
                "123",
                "4000000000000077",
                01,
                2025);
        }

        [Fact]
        public void EmptyTokenIsNotValid()
        {
            var isValid = AwsCognitoJwtTokenValidator.IsValid(string.Empty, string.Empty);
            output.WriteLine(isValid.ToString());
        }

        [Fact]
        public void GetInstitution()
        {
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());

            var ins = client.GetInstitution("ins_9")["institution"];
            output.WriteLine(ins.ToString(Formatting.Indented));


            var itemJson = new JArray();
            var institutionsJson = new JArray();
            var institutions = new HashSet<string>();
            var itemRecord = client.GetItem("")["item"];
            institutions.Add(itemRecord["institution_id"].Value<string>());
            itemJson.Add(itemRecord);
            foreach (var institution in institutions)
            {
                institutionsJson.Add(client.GetInstitution(institution)["institution"]);
            }
            foreach (var item in itemJson)
            {
                var institutionId = item["institution_id"].Value<string>();
                item["institution"] = institutionsJson.First(x =>
                    string.Equals(x["institution_id"].Value<string>(), institutionId, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public void GetItem()
        {
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());
            var balance = client.GetItem("");
            output.WriteLine(balance.ToString(Formatting.Indented));
        }

        [Fact]
        public void RemoveItem()
        {
            var client = new BankAccessClient(Configuration.PLAID_URL, new Logger());
            var balance = client.RemoveItem("");
            output.WriteLine(balance.ToString(Formatting.Indented));
        }

    }
}
