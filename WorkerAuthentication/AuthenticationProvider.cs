
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace azure_functions_dotnet_worker_miw.WorkerAuthentication
{
    public class AuthenticationProvider
    {
        private readonly AzureAdJwtBearerValidation _azureAdJwtBearerValidation;
        private static IConfidentialClientApplication _application;
        public AuthenticationProvider(IConfiguration configuration, AzureAdJwtBearerValidation azureAdJwtBearerValidation)
        {
            var tenantId = configuration["AzureAd:TenantId"];
            var clientId = configuration["AzureAd:ClientId"];
            var instance = configuration["AzureAd:Instance"];
            var clientSecret = configuration["AzureAd:ClientSecret"];

            _azureAdJwtBearerValidation = azureAdJwtBearerValidation;
            if(_application == null)
            {
                _application = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithAuthority($"{instance}{tenantId}/v2.0/")
                    .WithClientSecret(clientSecret)
                    .Build();
            }
        }
        public async Task<ClaimsPrincipal> AuthenticateAsync(FunctionContext context, HttpRequestData req)
        {
            return await _azureAdJwtBearerValidation.ValidateTokenAsync(getAccessTokenFromHeaders(req));
        }

        public async Task<string> GetAccessTokenForUserAsync(HttpRequestData req, IEnumerable<string> scopes, string? tenantId = null, string? userFlow = null)
        {
            var result = await _application.AcquireTokenOnBehalfOf(scopes, new UserAssertion(getAccessTokenFromHeaders(req))).ExecuteAsync();

            return result.AccessToken;
        }

        public HttpResponseData ReplyUnauthorized(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.Unauthorized);

            return response;
        }

        public HttpResponseData ReplyForbidden(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.Forbidden);

            return response;
        }

        private string getAccessTokenFromHeaders(HttpRequestData req)
        {
            var token = req.Headers.Where(x => x.Key == "Authorization").First().Value.First().Substring("Bearer ".Length);
            
            return token;
        }
    }
}