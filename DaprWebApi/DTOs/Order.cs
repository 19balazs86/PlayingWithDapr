namespace DaprWebApi.DTOs;

public sealed record Order(int Id, DateTime CreatedAtUtc)
{
    private static int _orderId = 0;

    public static Order CreateNew()
    {
        return new Order(Interlocked.Increment(ref _orderId), DateTime.UtcNow);
    }
}