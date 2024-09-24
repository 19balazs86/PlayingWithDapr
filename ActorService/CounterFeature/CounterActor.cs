using Dapr.Actors.Runtime;

namespace ActorService.CounterFeature;

// Quickstart Example: https://github.com/dapr/quickstarts/tree/master/actors/csharp/sdk
public sealed class CounterActor : Actor, ICounterActor
{
    private const string _statName = nameof(CounterState);

    private readonly string _name;

    public CounterActor(ActorHost host) : base(host)
    {
        _name = Host.Id.GetId();
    }

    public async Task<int> Add(int number, CancellationToken ct)
    {
        Logger.LogInformation("Counter({Name}) Add: {Number}", _name, number);

        CounterState state = await getState(ct);

        state.Value += number;
        state.LastCall = DateTime.UtcNow;

        await setState(state, ct);

        return state.Value;
    }

    public Task<CounterState> Get()
    {
        // ProxyFactory.CreateActorProxy<>() // You can use the ProxyFactory property to manage other actors

        return getState();
    }

    private async Task<CounterState> getState(CancellationToken ct = default)
    {
        return await StateManager.GetOrAddStateAsync(_statName, new CounterState(), ct);
    }

    private async Task setState(CounterState state, CancellationToken ct)
    {
        await StateManager.SetStateAsync(_statName, state, ct);
    }
}
