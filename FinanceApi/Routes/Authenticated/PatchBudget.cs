using System;
using System.Collections.Generic;
using System.Text;
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
            var updateItemResponse = new UserService().UpdateUser(user.Email, jsonPatch);
            var jsonResponse = JObject.Parse(Document.FromAttributeMap(updateItemResponse.Attributes).ToJson());
            jsonResponse.Remove("bankLinks");
            response.Body = jsonResponse.ToString();
            response.StatusCode = (int)updateItemResponse.HttpStatusCode;
        }
    }
}
