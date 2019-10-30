using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace BankDataAccess.Tests
{
    public class Test
    {
        private readonly ITestOutputHelper output;
        private HttpClient client = new HttpClient();

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// By default, the access_token associated with an Item does not expire and should be stored in a persistent, secure manner.
        /// You can use the POST /item/access_token/invalidate endpoint to rotate the access_token associated with an Item.The endpoint returns a new access_token and immediately invalidates the previous access_token.
        /// </summary>
        /// <remarks>
        /// Access token has been saved to environment variables.
        /// </remarks>
        // [Fact] Don't bother running until getting a new public token, because it expires after 30 minutes.
        public void Create_Access_Token()
        {
            // Use link form to get the pre-req data and authenticate to your bank: https://blog.plaid.com/plaid-link/
            var data = new JObject
            {
                {"client_id", PlaidConfiguration.DEV_CLIENT_ID },
                {"public_token", "REDACTED"}, // Get from Link form
                {"secret", PlaidConfiguration.DEV_SECRET }
            };
            var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            var url = PlaidConfiguration.DEV_URL + "/item/public_token/exchange";
            output.WriteLine(url);
            var postResult = client.PostAsync(url, content).Result;
            output.WriteLine( ((int) postResult.StatusCode).ToString());
            output.WriteLine(postResult.Content.ReadAsStringAsync().Result);
            postResult.EnsureSuccessStatusCode();
        }

        [Fact]
        public void CheckAccountBalance()
        {
            var client = new BankClient(PlaidConfiguration.DEV_URL);
            output.WriteLine(client.GetAccountBalance().ToString(Formatting.Indented));
        }

    }
}
