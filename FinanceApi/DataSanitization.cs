using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FinanceApi
{
    public class DataSanitization
    {
        public static void SanitizeOutput(JObject output)
        {
            if (output["bankLinks"] != null)
            {
                output.Remove("bankLinks");
            }
        }

        /// <summary>
        /// Names will be assigned while point of sale features are in this project for a strong identity.
        /// </summary>
        public static List<string> BlackListedFinanceUserInputs => new List<string>
        {
            "email", "firstName", "lastName", "licenseAgreement", "billingAgreement", "bankLinks"
        };

        public static void SanitizeInput(JObject input)
        {
            foreach (var blackListedFinanceUserInput in BlackListedFinanceUserInputs)
            {
                if (input[blackListedFinanceUserInput] != null)
                {
                    input.Remove(blackListedFinanceUserInput);
                }
            }
        }
    }
}
