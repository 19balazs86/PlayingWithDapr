using Azure.Storage.Queues;
using JobWorker.Miscellaneous;
using Microsoft.Extensions.Options;

namespace JobWorker.Workers;

public sealed class WorkerSender : WorkerBase
{
    public WorkerSender(
        ILogger<WorkerSender> logger,
        QueueServiceClient queueServiceClient,
        IOptions<WorkerSettings> workerOptions,
        IHostApplicationLifetime hostApplicationLifetime) : base(logger, queueServiceClient, workerOptions.Value, hostApplicationLifetime)
    {

    }

    protected override async Task doWorkAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sending {number} messages", _workerSettings.SendNumberOfMessages);

        JobMessage[] jobMessages = JobMessage.CreateMore(_workerSettings.SendNumberOfMessages).ToArray();

        for (int i = 0; i < _workerSettings.SendNumberOfMessages && !stoppingToken.IsCancellationRequested; i++)
        {
            BinaryData message = BinaryData.FromObjectAsJson(jobMessages[i]);

            await _queueClient.SendMessageAsync(message);

            await Task.Delay(500);
        }

        _logger.LogInformation("Messages are sent");
    }
}
