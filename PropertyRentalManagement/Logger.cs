using System;
using AwsTools;

namespace PropertyRentalManagement
{
    public class Logger : ILogging, ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
