using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using azure_functions_dotnet_worker_miw.WorkerAuthentication;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace azure_functions_dotnet_worker_miw
{
    public class HttpTrigger
    {
        private readonly AuthenticationProvider _authentication;
        public HttpTrigger(AuthenticationProvider authentication)
        {
            _authentication = authentication;
        }

        [Function("HttpTrigger")]
        public async Task<HttpResponseData> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpTrigger");

            var principal = await _authentication.AuthenticateAsync(executionContext, req);
            if(principal == null) return _authentication.ReplyUnauthorized(req);

            var microsoftGraph = new GraphServiceClient(new DelegateAuthenticationProvider(async (request) =>
            {
                var token = await _authentication.GetAccessTokenForUserAsync(req, new string[] { "https://graph.microsoft.com/.default" });
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                await Task.FromResult<object>(null);
            }));

            var name = principal.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").First().Value;
            logger.LogInformation($"User is {name}!");

            var inbox = await microsoftGraph.Me.MailFolders.Inbox.Request().GetAsync();
            logger.LogInformation($"{name} has {inbox.UnreadItemCount} unread e-mails!");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString($"Welcome to Azure Functions Out-Of-Process, {name}!\n\nYou have {inbox.UnreadItemCount} unread e-mails!");

            return response;
        }
    }
}
