using Azure.Storage.Queues;
using JobWorker.Miscellaneous;
using Microsoft.Extensions.Options;

namespace JobWorker.Workers;

public sealed class WorkerSender : BackgroundService
{
    private readonly ILogger<WorkerSender> _logger;

    private readonly QueueClient _queueClient;

    private readonly WorkerSettings _queueSettings;

    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public WorkerSender(
        ILogger<WorkerSender> logger,
        QueueServiceClient queueServiceClient,
        IOptions<WorkerSettings> queueOptions,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _logger = logger;

        _queueSettings = queueOptions.Value;

        _queueClient = queueServiceClient.GetQueueClient(_queueSettings.JobQueueName);

        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Sending {number} messages", _queueSettings.SendNumberOfMessages);

            await _queueClient.CreateIfNotExistsAsync();

            JobMessage[] jobMessages = JobMessage.CreateMore(_queueSettings.SendNumberOfMessages).ToArray();

            for (int i = 0; i < _queueSettings.SendNumberOfMessages && !stoppingToken.IsCancellationRequested; i++)
            {
                BinaryData message = BinaryData.FromObjectAsJson(jobMessages[i]);

                await _queueClient.SendMessageAsync(message);

                await Task.Delay(500);
            }

            _logger.LogInformation("Messages are sent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send messages");
        }
        finally
        {
            // Stop the application as it meant to be a Container App Job
            _hostApplicationLifetime.StopApplication();
        }
    }
}