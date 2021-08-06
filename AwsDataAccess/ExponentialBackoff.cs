using System;

namespace AwsDataAccess
{
    public class ExponentialBackoff
    {
        public static TimeSpan GetWaitTime(int pastFailedAttempts, TimeSpan baseDelay, TimeSpan maxDelay)
        {
            var exponentialWaitTime = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, pastFailedAttempts));
            return exponentialWaitTime < maxDelay
                ? exponentialWaitTime
                : maxDelay;
        }
    }
}
