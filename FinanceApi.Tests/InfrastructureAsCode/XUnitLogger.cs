using PropertyRentalManagement;
using AwsTools;
using Xunit.Abstractions;

namespace Tests
{
    class XUnitLogger : ILogger, ILogging
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
