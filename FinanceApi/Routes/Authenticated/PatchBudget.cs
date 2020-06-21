using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using FinanceApi.DatabaseModel;
using Newtonsoft.Json.Linq;

namespace FinanceApi.Routes.Authenticated
{
    class PatchBudget : IRoute
    {
        public string HttpMethod => "PATCH";
        public string Path => "/budget";
        public void Run(APIGatewayProxyRequest request, APIGatewayProxyResponse response, FinanceUser user)
        {
            var jsonPatch = JObject.Parse(request.Body);
            DataSanitization.SanitizeInput(jsonPatch);
            var updateItemResponse = new UserService().UpdateUser(user.Email, jsonPatch);
            var jsonResponse = JObject.Parse(Document.FromAttributeMap(updateItemResponse.Attributes).ToJson());
            DataSanitization.SanitizeOutput(jsonResponse);
            response.Body = jsonResponse.ToString();
            response.StatusCode = (int)updateItemResponse.HttpStatusCode;
        }
    }
}
