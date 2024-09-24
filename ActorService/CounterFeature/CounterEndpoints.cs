using Dapr.Actors;
using Dapr.Actors.Client;
using System.Collections.Concurrent;

namespace ActorService.CounterFeature;

public static class CounterEndpoints
{
    public static void MapCounterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/counter");

        group.MapGet("/{name}/state", getState);
        group.MapGet("/add", handleAdd);
    }

    private static async Task<CounterState> getState(string name, IActorProxyFactory actorProxyFactory)
    {
        var actorId = new ActorId(name);

        // On the client side, the ActorProxy can be used to create actors using the package: Dapr.Actors
        // ActorProxy.Create<ICounterActor>(actorId, ICounterActor.ActorType);

        ICounterActor counter = actorProxyFactory.CreateActorProxy<ICounterActor>(actorId, ICounterActor.ActorType);

        return await counter.Get();
    }

    private static async Task<IDictionary<string, int>> handleAdd(IActorProxyFactory actorProxyFactory, CancellationToken cancelToken)
    {
        string[] counterNames = Enumerable.Range(0, 10).Select(_ => $"MyCounter{Random.Shared.Next(1, 21):D2}").ToArray();

        await Parallel.ForEachAsync(counterNames, cancelToken, async (counterName, ct) =>
        {
            var actorId = new ActorId(counterName);

            ICounterActor counter = actorProxyFactory.CreateActorProxy<ICounterActor>(actorId, ICounterActor.ActorType);

            await counter.Add(Random.Shared.Next(-20, 20), ct);
        });

        ConcurrentDictionary<string, int> concurrentDictionary = [];

        await Parallel.ForEachAsync(counterNames.Distinct(), async (counterName, ct) =>
        {
            var actorId = new ActorId(counterName);

            ICounterActor counter = actorProxyFactory.CreateActorProxy<ICounterActor>(actorId, ICounterActor.ActorType);

            CounterState counterState = await counter.Get();

            concurrentDictionary[counterName] = counterState.Value;
        });

        return concurrentDictionary;
    }
}