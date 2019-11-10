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

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CheckAccountBalance()
        {
            var client = new BankClient(PlaidConfiguration.DEV_URL);
            var balance = client.GetAccounts(PlaidConfiguration.DEV_ACCESS_TOKEN_PERSONAL_CHECKING);
            output.WriteLine(balance.ToString(Formatting.Indented));
        }

    }
}
