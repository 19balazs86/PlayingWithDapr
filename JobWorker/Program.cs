using Azure.Core;
using Azure.Identity;
using JobWorker.Miscellaneous;
using JobWorker.Workers;
using Microsoft.Extensions.Azure;

namespace JobWorker;

/*
* This worker service is responsible for sending and receiving messages from the Storage Queue
*
* Based on the value of IsShortRunningJob in the settings
* - True:  It could be a Container Job
* - False: It could be a long-running Container App with a scale rule of 0-X
*/
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        WorkerSettings queueSettings = builder.ConfigureAndReturn<WorkerSettings>()!;
        IServiceCollection services = builder.Services;

        // Add services to the container
        {
            services.AddHostedService<WorkerSender>();

            services.AddAzureClients(clients =>
            {
                if (builder.Configuration.GetValue<bool>("UseAzurite"))
                {
                    clients.AddQueueServiceClient("UseDevelopmentStorage=true");
                }
                else
                {
                    // Role assignment: "Storage Queue Data Contributor"
                    // Instead of the full connection string, use the DefaultAzureCredential, as the service has a UserAssigned identity
                    clients.AddQueueServiceClient(queueSettings.QueueEndpointUri);

                    // I am experiencing an issue when using DefaultAzureCredential on my machine.
                    // However, it works fine when I create a QueueClient with DefaultAzureCredential directly.
                    // This issue only occurs when I use services.AddAzureClients
                    TokenCredential tokenCredential = builder.Environment.IsDevelopment() ?
                        new AzureCliCredential() :
                        new DefaultAzureCredential();

                    clients.UseCredential(tokenCredential);
                }
            });
        }

        IHost host = builder.Build();

        host.Run();
    }
}