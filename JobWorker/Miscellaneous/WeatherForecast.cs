namespace JobWorker.Miscellaneous;

public sealed class WeatherForecast
{
    private static readonly string[] _summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private static readonly string[] _cities =
    [
        "London", "Budapest", "Paris", "Dublin", "Zurich", "Los Angeles", "New York"
    ];

    public required string City { get; init; }
    public DateOnly Date { get; init; }
    public int Temperature { get; init; }
    public required string Summary { get; init; }

    public static IEnumerable<WeatherForecast> CreateMore(int number)
    {
        for (int i = 0; i < number; i++)
        {
            yield return Create(i);
        }
    }

    public static WeatherForecast Create(int addDay = 1)
    {
        return new WeatherForecast
        {
            City        = _cities[Random.Shared.Next(_cities.Length)],
            Date        = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(addDay)),
            Temperature = Random.Shared.Next(-20, 55),
            Summary     = _summaries[Random.Shared.Next(_summaries.Length)]
        };
    }
}

