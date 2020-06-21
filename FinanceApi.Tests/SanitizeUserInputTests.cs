using Newtonsoft.Json.Linq;
using Xunit;

namespace FinanceApi.Tests
{
    public class SanitizeUserInputTests
    {
        [Fact]
        public void Bank_Links_Output_Is_Sanitized()
        {
            var input = new JObject {{"bankLinks", "test"}};
            Assert.NotNull(input["bankLinks"]);
            DataSanitization.SanitizeOutput(input);
            Assert.Null(input["bankLinks"]);
        }

        [Fact]
        public void Email_Input_Is_Sanitized()
        {
            var input = new JObject {{"email", "test"}};
            Assert.NotNull(input["email"]);
            DataSanitization.SanitizeInput(input);
            Assert.Null(input["email"]);
        }
    }
}
