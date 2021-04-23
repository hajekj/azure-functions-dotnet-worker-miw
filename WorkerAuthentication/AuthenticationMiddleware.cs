using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace azure_functions_dotnet_worker_miw.WorkerAuthentication
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly AuthenticationProvider _authenticationProvider;
        public AuthenticationMiddleware(AuthenticationProvider authenticationProvider)
        {
            _authenticationProvider = authenticationProvider;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var logger = context.GetLogger<AuthenticationMiddleware>();
            var data = context.BindingContext.BindingData;

            if(data.ContainsKey("Headers"))
            {
                data.TryGetValue("Headers", out var headersObject);

                var headers = JsonSerializer.Deserialize<JsonDocument>(headersObject as string);

                var authzHeaderExists = headers.RootElement.TryGetProperty("Authorization", out JsonElement authorizationHeader);

                if(authzHeaderExists)
                {
                    var token = authorizationHeader.ToString().Substring("Bearer ".Length);

                    var principal = await _authenticationProvider.AuthenticateAsync(context, token);
                    if (principal != null)
                    {

                        await next(context);
                        return;
                    }
                }
            }

            // return 401
        }
    }
}