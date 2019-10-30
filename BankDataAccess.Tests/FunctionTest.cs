using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json.Linq;
using Xunit;

namespace BankDataAccess.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestToUpperFunction()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var upperCase = function.FunctionHandler(new JObject(), context);
        }

    }
}
