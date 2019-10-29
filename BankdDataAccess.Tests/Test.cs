using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace MemexLambdaCrawlerTests
{
    public class Test
    {
        private readonly ITestOutputHelper output;
        private HttpClient client = new HttpClient();
        private const string DEV_URL = "https://development.plaid.com";

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// By default, the access_token associated with an Item does not expire and should be stored in a persistent, secure manner.
        /// You can use the POST /item/access_token/invalidate endpoint to rotate the access_token associated with an Item.The endpoint returns a new access_token and immediately invalidates the previous access_token.
        /// </summary>
        /// <remarks>
        /// Access token has been saved to environment variables.
        /// </remarks>
        // [Fact] Don't bother running until getting a new public token, because it expires after 30 minutes.
        public void Create_Access_Token()
        {
            // Use link form to get the pre-req data and authenticate to your bank: https://blog.plaid.com/plaid-link/
            var data = new JObject
            {
                {"client_id", PlaidConfiguration.DEV_CLIENT_ID },
                {"public_token", "REDACTED"}, // Get from Link form
                {"secret", PlaidConfiguration.DEV_SECRET }
            };
            var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            var url = DEV_URL + "/item/public_token/exchange";
            output.WriteLine(url);
            var postResult = client.PostAsync(url, content).Result;
            output.WriteLine( ((int) postResult.StatusCode).ToString());
            output.WriteLine(postResult.Content.ReadAsStringAsync().Result);
            postResult.EnsureSuccessStatusCode();
        }

        [Fact]
        public void CheckAccountBalance()
        {
            var data = new JObject
            {
                {"client_id", PlaidConfiguration.DEV_CLIENT_ID },
                {"secret", PlaidConfiguration.DEV_SECRET },
                { "access_token", PlaidConfiguration.DEV_ACCESS_TOKEN_PERSONAL_CHECKING }
            };
            var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            var url = DEV_URL + "/accounts/balance/get";
            var postResult = client.PostAsync(url, content).Result;
            output.WriteLine(((int)postResult.StatusCode).ToString());
            output.WriteLine(postResult.Content.ReadAsStringAsync().Result);
            postResult.EnsureSuccessStatusCode();
        }

        [Fact]
        public void Check_For_Secrets_In_Source_Code()
        {
            var files = Directory.EnumerateFiles(
                "C:\\Users\\peon\\Desktop\\projects\\bank-data-access",
                "*.*",
                SearchOption.AllDirectories
            ).ToList();
            Assert.True(files.Count > 0);

            IList<string> secrets = new List<string>();
            foreach (string secret in Environment.GetEnvironmentVariables().Values)
            {
                if (!string.Equals("na", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("default", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("6", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("8", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("16.0", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("-4", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("c:", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("random", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("c:\\program files", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("c:\\users\\random", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("1m", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals("true", secret, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(secret))
                {
                    secrets.Add(secret);
                }
            }

            int filesChecked = 0;
            foreach (string file in files)
            {
                try
                {
                    var sourceCode = File.ReadAllText(file, Encoding.UTF8);
                    foreach (var secret in secrets)
                    {
                        CheckForSecret(sourceCode, file, secret);
                    }
                    filesChecked += 1;
                }
                catch (IOException e)
                {
                    if (e.Message.Contains("\\.vs\\")) // This whole folder is gitignored.
                    {
                        Console.WriteLine("Skipped visual studio file: " + e.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            Console.WriteLine($"checked {filesChecked} of {files.Count} files.");
        }

        private void CheckForSecret(string sourceCode, string sourceCodeFile, string secret)
        {
            secret = secret.ToLower();
            if (secret.Equals("snapshots", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            sourceCode = sourceCode.ToLower();
            if (sourceCode.Contains(secret))
            {
                throw new Exception($"Secret {secret} discovered in source code at: " + sourceCodeFile);
            }
        }
        
    }
}
