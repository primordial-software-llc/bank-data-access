using AwsTools;
using Xunit.Abstractions;

namespace FinanceApi.Tests.InfrastructureAsCode
{
    class XUnitLogger : ILogging
    {
        private ITestOutputHelper Output { get; }

        public XUnitLogger(ITestOutputHelper output)
        {
            Output = output;
        }

        public void Log(string message)
        {
            Output.WriteLine(message);
        }
    }
}
