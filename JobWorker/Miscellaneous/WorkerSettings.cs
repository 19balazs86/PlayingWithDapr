namespace JobWorker.Miscellaneous;

public sealed class WorkerSettings : IConfigOptions
{
    public static string SectionName => nameof(WorkerSettings);

    public required string QueueEndpointUrl { get; init; }
    public required string JobQueueName { get; init; }
    public required bool IsShortRunningJob { get; init; }
    public required int SendNumberOfMessages { get; init; }

    public Uri QueueEndpointUri => new Uri(QueueEndpointUrl);
}
