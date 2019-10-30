using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace BankDataAccess.Tests
{
    public class SecretsInSourceCodeTest
    {
        [Fact]
        public void Check_For_Secrets_In_Source_Code()
        {
            var gitignoredFiles = new List<string>
            {
                "BankDataAccess/aws-lambda-tools-defaults.json"
            };
            var notSecrets = new List<string>
            {
                "na", "default", "6", "8", "16.0", "-4",
                "c:", "random", "c:\\program files",
                "c:\\users\\random", "1m", "true", "1"
            };
            const string PROJECT_ROOT = "C:\\Users\\peon\\Desktop\\projects\\bank-data-access\\";
            var files = Directory.EnumerateFiles(PROJECT_ROOT, "*.*", SearchOption.AllDirectories).ToList();
            Assert.True(files.Count > 0);

            var gitIgnore = File.ReadAllLines("C:\\Users\\peon\\Desktop\\projects\\bank-data-access\\.gitignore");
            foreach (var gitignoredFile in gitignoredFiles)
            {
                Assert.Contains(gitignoredFile, gitIgnore);
                files = files.Where(x => !string.Equals(x, PROJECT_ROOT + gitignoredFile.Replace("/", "\\"), StringComparison.OrdinalIgnoreCase)).ToList();
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
            sourceCode = sourceCode.ToLower();
            if (sourceCode.Contains(secret))
            {
                throw new Exception($"Secret {secret} discovered in source code at: " + sourceCodeFile);
            }
        }
    }
}
