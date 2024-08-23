using Azure.Storage.Queues;
using JobWorker.Miscellaneous;

namespace JobWorker.Workers;

public abstract class WorkerBase : BackgroundService
{
    protected readonly ILogger _logger;

    protected readonly QueueClient _queueClient;

    protected readonly WorkerSettings _workerSettings;

    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public WorkerBase(
        ILogger logger,
        QueueServiceClient queueServiceClient,
        WorkerSettings workerSettings,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _logger = logger;

        _workerSettings = workerSettings;

        _queueClient = queueServiceClient.GetQueueClient(_workerSettings.JobQueueName);

        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected abstract Task doWorkAsync(CancellationToken stoppingToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _queueClient.CreateIfNotExistsAsync();

            do
            {
                await doWorkAsync(stoppingToken);

                if (_workerSettings.IsLongRunningApp)
                {
                    await Task.WhenAny(Task.Delay(5_000, stoppingToken));
                }
            } while (_workerSettings.IsLongRunningApp && !stoppingToken.IsCancellationRequested);
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
