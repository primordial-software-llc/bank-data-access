using System;
using AwsTools;

namespace BankDataAccess
{
    public class Logger : ILogging
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
