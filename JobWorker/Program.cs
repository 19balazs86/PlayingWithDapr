using Azure.Core;
using Azure.Identity;
using JobWorker.Miscellaneous;
using JobWorker.Workers;
using Microsoft.Extensions.Azure;

namespace JobWorker;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        QueueSettings queueSettings = builder.ConfigureAndReturn<QueueSettings>()!;
        IServiceCollection services = builder.Services;

        // Add services to the container
        {
            services.AddHostedService<WorkerSender>();

            services.AddAzureClients(clients =>
            {
                // Role assignment: "Storage Queue Data Contributor"
                // Instead of the full connection string, use the DefaultAzureCredential, as the service has a UserAssigned identity
                clients.AddQueueServiceClient(queueSettings.ServiceUri);

                // I am experiencing an issue when using DefaultAzureCredential on my machine.
                // However, it works fine when I create a QueueClient with DefaultAzureCredential directly.
                // This issue only occurs when I use services.AddAzureClients
                TokenCredential tokenCredential = builder.Environment.IsDevelopment() ?
                    new AzureCliCredential() :
                    new DefaultAzureCredential();

                clients.UseCredential(tokenCredential);
            });
        }

        IHost host = builder.Build();

        host.Run();
    }
}