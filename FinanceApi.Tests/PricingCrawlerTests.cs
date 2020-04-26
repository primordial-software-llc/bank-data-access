using System;
using System.Net;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace FinanceApi.Tests
{
    public class PricingCrawlerTests
    {
        private readonly ITestOutputHelper output;

        public PricingCrawlerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Run()
        {
            var result = GetPageHtml("khc");
            output.WriteLine(result);
        }

        public static string GetPageHtml(string tickerSymbol)
        {
            string url = $"https://finance.yahoo.com/quote/{tickerSymbol}";
            string html;
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                html = wc.DownloadString(url);
            }
            return html;
        }

        public static string GetBetween(string data, string start, string end)
        {
            var startIndex = data.IndexOf(start, StringComparison.OrdinalIgnoreCase);

            if (startIndex == -1)
            {
                return string.Empty;
            }

            var endIndex = data.IndexOf(end, startIndex, StringComparison.OrdinalIgnoreCase);

            if (startIndex == -1 || endIndex == -1)
            {
                return string.Empty;
            }

            var dataBetween = data.Substring(startIndex + start.Length, endIndex - startIndex - start.Length);
            return dataBetween;
        }
    }
}
