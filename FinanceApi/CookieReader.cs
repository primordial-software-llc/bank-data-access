using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;

namespace FinanceApi
{
    class CookieReader
    {
        public static string GetCookie(APIGatewayProxyRequest request, string cookieName)
        {
            var cookieHeader = request.MultiValueHeaders.Keys.FirstOrDefault(x => string.Equals(x, "cookie", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                return string.Empty;
            }
            var cookieValues = request.MultiValueHeaders[cookieHeader];
            foreach (var cookieValue in cookieValues)
            {
                var idTokenCookie = cookieValue.Split(';')
                    .Select(x => x.Trim())
                    .FirstOrDefault(x => string.Equals(x.Split("=")[0], cookieName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(idTokenCookie) && idTokenCookie.Split("=").Length > 0)
                {
                    return idTokenCookie.Split("=")[1];
                }
            }

            return string.Empty;
        }
    }
}
