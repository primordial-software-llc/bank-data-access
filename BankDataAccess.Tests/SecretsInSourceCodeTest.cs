using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace BankDataAccess.Tests
{
    public class SecretsInSourceCodeTest
    {
        private readonly ITestOutputHelper output;

        public SecretsInSourceCodeTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        const string PROJECT_ROOT = "C:\\Users\\peon\\Desktop\\projects\\bank-data-access\\";
        [Fact]
        public void Check_For_Secrets_In_Source_Code()
        {
            var gitIgnoredFiles = new List<string>
            {
                "BankDataAccess/aws-lambda-tools-defaults.json"
            };
            var notSecrets = new List<string>
            {
                "na", "default", "6", "8", "16.0", "-4",
                "c:", "random", "c:\\program files",
                "c:\\users\\random", "1m", "true", "1"
            };
            var files = Directory.EnumerateFiles(PROJECT_ROOT, "*.*", SearchOption.AllDirectories).ToList();
            Assert.True(files.Count > 0);

            var gitIgnore = File.ReadAllLines("C:\\Users\\peon\\Desktop\\projects\\bank-data-access\\.gitignore");
            foreach (var gitIgnoredFile in gitIgnoredFiles)
            {
                Assert.Contains(gitIgnoredFile, gitIgnore);
                files = files.Where(x => !string.Equals(x, PROJECT_ROOT + gitIgnoredFile.Replace("/", "\\"), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            IList<string> secrets = new List<string>();
            foreach (string secret in Environment.GetEnvironmentVariables().Values)
            {
                if (notSecrets.All(notSecret => string.Equals(notSecret, secret, StringComparison.OrdinalIgnoreCase)) &&
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
                        output.WriteLine("Skipped visual studio file: " + e.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            output.WriteLine($"checked {filesChecked} of {files.Count} files. {files.Count - filesChecked} files were git ignored.");
            output.WriteLine(DateTime.Now.ToString());
        }

        private void CheckForSecret(string sourceCode, string sourceCodeFile, string secret)
        {
            secret = secret.ToLower();
            sourceCode = sourceCode.ToLower();
            if (sourceCode.Contains(secret))
            {
                throw new Exception($"Secret {secret} discovered in source code at: " + sourceCodeFile);
            }
        }
    }
}
