namespace JobWorker.Miscellaneous;

public sealed class QueueSettings : IConfigOptions
{
    public static string SectionName => nameof(QueueSettings);

    public required string ServiceUrl { get; init; }
    public required string JobQueueName { get; init; }
    public required int SendNumberOfMessages { get; init; }

    public Uri ServiceUri => new Uri(ServiceUrl);
}
