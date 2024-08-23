using Azure.Core;
using Azure.Identity;
using JobWorker.Miscellaneous;
using JobWorker.Workers;
using Microsoft.Extensions.Azure;

namespace JobWorker;

/*
* This worker service is responsible for sending and receiving messages from a Storage Queue
*
* Based on the value of IsShortRunningJob in the settings
* - True:  It could be a Container Job
* - False: It could be a long-running Container App with a scale rule
*/
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        WorkerSettings workerSettings = builder.ConfigureAndReturn<WorkerSettings>()!;
        IServiceCollection services   = builder.Services;
        IConfiguration configuration  = builder.Configuration;

        // Add services to the container
        {
            services.addWorker_Sender_or_Receiver(workerSettings);

            services.AddAzureClients(clients =>
            {
                if (builder.Configuration.GetValue<bool>("UseConnString"))
                {
                    clients.AddQueueServiceClient(configuration.GetConnectionString("StorageAccount"));
                }
                else
                {
                    // Role assignment: "Storage Queue Data Contributor"
                    // Instead of the full connection string, use the DefaultAzureCredential, as the service has a UserAssigned identity
                    clients.AddQueueServiceClient(workerSettings.QueueEndpointUri);

                    // I am experiencing an issue when using DefaultAzureCredential on my machine
                    // However, it works fine when I manually create a QueueServiceClient or QueueClient passing them the DefaultAzureCredential
                    // This issue only occurs when I use services.AddAzureClients and QueueServiceClient.GetQueueClient
                    TokenCredential tokenCredential = builder.Environment.IsDevelopment() ?
                        new DefaultAzureCredential(_credentialOptions) :
                        new DefaultAzureCredential();

                    clients.UseCredential(tokenCredential);
                }
            });
        }

        IHost host = builder.Build();

        host.Run();
    }

    private static void addWorker_Sender_or_Receiver(this IServiceCollection services, WorkerSettings workerSettings)
    {
        if (workerSettings.SendNumberOfMessages > 0)
        {
            services.AddHostedService<WorkerSender>();
        }
        else
        {
            services.AddHostedService<WorkerReceiver>();
        }
    }

    private readonly static DefaultAzureCredentialOptions _credentialOptions = new DefaultAzureCredentialOptions
    {
        ExcludeEnvironmentCredential      = true,
        ExcludeWorkloadIdentityCredential = true,
        ExcludeManagedIdentityCredential  = true,
    };
}