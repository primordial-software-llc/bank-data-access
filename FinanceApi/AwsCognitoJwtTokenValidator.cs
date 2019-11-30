using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace FinanceApi
{
    /// <summary>
    /// I'll keep this here just for one commit so I have it documented, but API gateway can do this validation so it's not needed if staying within AWS's products.
    /// Feel free to remove this once it's in source control.
    /// </summary>
    public class AwsCognitoJwtTokenValidator
    {
        public static bool IsValid(string token, string cognitoUserPoolId)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }
            var tokenHeader = Encoding.UTF8.GetString(AwsCognitoJwtTokenValidator.FromBase64Url(token.Split(".")[0]));
            var tokenKeyId = JObject.Parse(tokenHeader)["kid"].Value<string>();

            var publicKeys = new HttpClient().GetStringAsync($"https://cognito-idp.us-east-1.amazonaws.com/{cognitoUserPoolId}/.well-known/jwks.json").Result;
            var json = JObject.Parse(publicKeys);
            var key = json["keys"].ToList().First(x => string.Equals(x["kid"].Value<string>(), tokenKeyId, StringComparison.OrdinalIgnoreCase));

            return IsValid(token, key["n"].Value<string>(), key["e"].Value<string>());
        }
        /// <summary>
        /// Split the first (header) and second (payload) parts of the jwt token.
        /// Use the header's key id to find the cognito user pools appropriate public key, downloaded from aws e.g. https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/jwks.json
        /// then use the n and e parameters to validate the signature.
        ///
        /// https://aws.amazon.com/premiumsupport/knowledge-center/decode-verify-cognito-json-token/
        /// </summary>
        /// <param name="token">JWT token or authorization header</param>
        /// <param name="publicKeyModulus">n parameter in decoded public key</param>
        /// <param name="publicKeyExponent">e parameter in decoded public key</param>
        /// <returns></returns>
        public static bool IsValid(string token, string publicKeyModulus, string publicKeyExponent)
        {
            string[] tokenParts = token.Split('.');

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(
                new RSAParameters
                {
                    Modulus = FromBase64Url(publicKeyModulus),
                    Exponent = FromBase64Url(publicKeyExponent)
                });

            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenParts[0] + '.' + tokenParts[1]));

            var payloadJson = JObject.Parse(Encoding.UTF8.GetString(FromBase64Url(tokenParts[1])));
            if (!payloadJson["email_verified"].Value<bool>())
            {
                return false;
            }

            var expirationSecondsFromUnixEpoch = payloadJson["exp"].Value<double>();
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (t.TotalSeconds > expirationSecondsFromUnixEpoch)
            {
                return false;
            }

            RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm("SHA256");
            return rsaDeformatter.VerifySignature(hash, FromBase64Url(tokenParts[2]));
        }

        public static byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0
                ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/")
                .Replace("-", "+");
            return Convert.FromBase64String(base64);
        }
    }
}
