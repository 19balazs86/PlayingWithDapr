namespace ActorService.CounterFeature;

public sealed class CounterState
{
    public int Value { get; set; }

    public DateTime? LastCall { get; set; }
}
