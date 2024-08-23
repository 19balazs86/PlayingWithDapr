using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using JobWorker.Miscellaneous;
using Microsoft.Extensions.Options;

namespace JobWorker.Workers;

public sealed class WorkerReceiver : WorkerBase
{
    public WorkerReceiver(
        ILogger<WorkerReceiver> logger,
        QueueServiceClient queueServiceClient,
        IOptions<WorkerSettings> workerOptions,
        IHostApplicationLifetime hostApplicationLifetime) : base(logger, queueServiceClient, workerOptions.Value, hostApplicationLifetime)
    {

    }

    protected override async Task doWorkAsync(CancellationToken stoppingToken)
    {
        QueueMessage[] queueMessages = [];

        do
        {
            // QueueProperties queueProperties = await _queueClient.GetPropertiesAsync(); // ApproximateMessagesCount property can be used for something
            // QueueMessage? queueMessage = await _queueClient.ReceiveMessageAsync();
            // PeekedMessage? peekedMessage = await _queueClient.PeekMessageAsync(); // Retrieves a message but does not alter the visibility

            // When the message is received, it is assigned a visibility timeout.
            // Giving us that time to process it before it becomes visible again and can be retrieved by other consumers
            queueMessages = await _queueClient.ReceiveMessagesAsync(maxMessages: 2, visibilityTimeout: TimeSpan.FromSeconds(30), stoppingToken);

            _logger.LogInformation("{number} messages have been received", queueMessages.Length);

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
    }
}
