using Dapr.Actors;

namespace ActorInterfaces;

public interface ICounterActor : IActor
{
    static readonly string ActorType = "CounterActor";

    Task<int> Add(int number, CancellationToken ct);

    Task<CounterState> Get();
}
