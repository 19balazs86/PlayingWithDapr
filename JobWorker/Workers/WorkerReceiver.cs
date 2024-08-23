using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using JobWorker.Miscellaneous;
using Microsoft.Extensions.Options;

namespace JobWorker.Workers;

public sealed class WorkerReceiver : BackgroundService
{
    private readonly ILogger<WorkerReceiver> _logger;

    private readonly QueueClient _queueClient;

    private readonly WorkerSettings _queueSettings;

    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public WorkerReceiver(
        ILogger<WorkerReceiver> logger,
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
            await _queueClient.CreateIfNotExistsAsync();

            _logger.LogInformation("Start receiving messages");

            // QueueProperties queueProperties = await _queueClient.GetPropertiesAsync(); // ApproximateMessagesCount property can be used for something
            // QueueMessage? jobMessage = await _queueClient.ReceiveMessageAsync();

            do
            {
                QueueMessage[] queueMessages = [];

                do
                {
                    // When the message is received, it is assigned a visibility timeout.
                    // Giving us that time to process it before it becomes visible again and can be retrieved by other consumers
                    queueMessages = await _queueClient.ReceiveMessagesAsync(maxMessages: 2, visibilityTimeout: TimeSpan.FromSeconds(30), stoppingToken);

                    foreach (QueueMessage queueMessage in queueMessages)
                    {
                        JobMessage jobMessage = queueMessage.Body.ToObjectFromJson<JobMessage>();

                        _logger.LogInformation("Processing: '{message}'", jobMessage);

                        await Task.Delay(500); // Process it

                        // After the message is processed, it can be deleted
                        await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, stoppingToken);
                    }
                }
                while (queueMessages.Length > 0);

                if (_queueSettings.IsLongRunningApp)
                {
                    await Task.WhenAny(Task.Delay(5_000, stoppingToken));
                }
            } while (_queueSettings.IsLongRunningApp && !stoppingToken.IsCancellationRequested);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong");
        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}
