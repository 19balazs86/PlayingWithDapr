namespace JobWorker.Miscellaneous;

public sealed record JobMessage(int Id, string City)
{
    private static int _messageId = 0;

    private static readonly string[] _cities =
    [
        "London", "Budapest", "Paris", "Dublin", "Zurich", "Los Angeles", "New York"
    ];

    public static JobMessage CreateNew()
    {
        return new JobMessage(Interlocked.Increment(ref _messageId), _cities[Random.Shared.Next(_cities.Length)]);
    }

    public static IEnumerable<JobMessage> CreateMore(int number)
    {
        for (int i = 0; i < number; i++)
        {
            yield return CreateNew();
        }
    }
}

