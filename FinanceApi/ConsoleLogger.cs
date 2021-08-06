using System;
using AwsDataAccess;

namespace FinanceApi
{
    public class ConsoleLogger : ILogging
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
