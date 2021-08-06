using System;
using System.Threading;

namespace AwsDataAccess
{
    public class Sleeper
    {
        private readonly ILogging logging;

        public Sleeper(ILogging logging)
        {
            this.logging = logging;
        }

        public void Sleep(TimeSpan length)
        {
            logging.Log($"Waiting {length.TotalMilliseconds}ms");
            Thread.Sleep(length);
        }
    }
}
