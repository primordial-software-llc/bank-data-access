using System;
using AwsTools;

namespace FinanceApi
{
    public class Logger : ILogging
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
