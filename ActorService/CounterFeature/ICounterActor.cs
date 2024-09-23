using Dapr.Actors;

namespace ActorService.CounterFeature;

// For simplicity, I placed the interface in the same project as the implementation
// However, it should be in a separate class library using the Dapr.Actors package
// The 'actor clients' can then use the interface to communicate with the actors using the ActorProxy class

public interface ICounterActor : IActor
{
    static readonly string ActorType = "CounterActor";

    Task<int> Add(int number, CancellationToken ct);

    Task<CounterState> Get();
}
