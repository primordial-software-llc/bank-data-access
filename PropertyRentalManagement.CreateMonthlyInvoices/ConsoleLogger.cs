using System;
using AwsTools;

namespace PropertyRentalManagement.CreateMonthlyInvoices
{
    public class ConsoleLogger : ILogging
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
