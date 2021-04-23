using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Configuration;
using azure_functions_dotnet_worker_miw.WorkerAuthentication;
using Microsoft.Extensions.DependencyInjection;

namespace azure_functions_dotnet_worker_miw
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(workerApplication =>
                {
                    // workerApplication.UseMiddleware<AuthenticationMiddleware>();
                })
                .ConfigureAppConfiguration(configuration =>
                {
                    configuration.AddUserSecrets<Program>(true);
                })
                .ConfigureServices(services =>
                {
                    services.AddScoped<AzureAdJwtBearerValidation>();
                    services.AddScoped<AuthenticationProvider>();
                })
                .Build();

            host.Run();
        }
    }
}