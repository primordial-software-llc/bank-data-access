using System;
using AwsTools;

namespace PropertyRentalManagement.CreateWeeklyInvoices
{
    public class ConsoleLogger : ILogging
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
