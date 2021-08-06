using System;
using AwsDataAccess;

namespace PropertyRentalManagement
{
    public class ConsoleLogger : ILogging
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
