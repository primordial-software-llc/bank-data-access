using Amazon.Lambda.APIGatewayEvents;

namespace FinanceApi
{
    public class IpLookup
    {
        public static string GetIp(APIGatewayProxyRequest request)
        {
            var ip = request.RequestContext.Identity.SourceIp;
            if (request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                ip = request.Headers["CF-Connecting-IP"];
            }
            return ip
        }
    }
}
