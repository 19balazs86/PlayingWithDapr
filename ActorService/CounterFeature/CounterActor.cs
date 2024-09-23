using Dapr.Actors.Runtime;

namespace ActorService.CounterFeature;

// Quickstart Example: https://github.com/dapr/quickstarts/tree/master/actors/csharp/sdk
public sealed class CounterActor : Actor, ICounterActor
{
    private const string _statName = nameof(CounterState);

    private readonly string _name;

    private CounterState? _counterState;

    public CounterActor(ActorHost host) : base(host)
    {
        _name = Host.Id.GetId();
    }

    public async Task<int> Add(int number, CancellationToken ct)
    {
        if (_counterState is null)
        {
            throw new NullReferenceException("The _counterState field is not inicialized");
        }

        Logger.LogInformation("Counter({Name}) Add: {Number}", _name, number);

        _counterState.Value += number;
        _counterState.LastCall = DateTime.UtcNow;

        await StateManager.SetStateAsync(_statName, _counterState, ct);

        return _counterState.Value;
    }

    public Task<CounterState> Get()
    {
        // ProxyFactory.CreateActorProxy<>() // You can use the ProxyFactory property to manage other actors

        return Task.FromResult(_counterState!);
    }

    protected override async Task OnActivateAsync()
    {
        _counterState = await StateManager.GetOrAddStateAsync(_statName, new CounterState());
    }
}
