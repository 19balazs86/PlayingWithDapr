using Dapr.Client;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DaprWebApi.Endpoints;

// Examples
// - HTTP and SDK: https://github.com/dapr/quickstarts/tree/master/state_management/csharp
// - Transactions, ETags, Bulk: https://github.com/dapr/dotnet-sdk/tree/master/examples/Client/StateManagement
public sealed class StateEndpoints : IEndpoint
{
    private const string _storeName = "my-state-store";
    private const string _stateKey  = "test-key";

    private static readonly Dictionary<string, string> _metadata = new() { ["ttlInSeconds"] = "120" }; // TTL set to 2 minutes

    private readonly DaprClient _daprClient;

    public StateEndpoints(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/state");

        group.MapGet("/",    getState);
        group.MapPost("/",   postState);
        group.MapDelete("/", deleteState);

        group.MapGet("/with-etag",  getStateAndETag);
        group.MapPost("/with-etag", postStateWithETag);
    }

    private async Task<Results<Ok<StateObject>, NoContent>> getState()
    {
        StateObject? stateObject = await _daprClient.GetStateAsync<StateObject?>(_storeName, _stateKey);

        if (stateObject is null)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(stateObject);
    }

    private async Task postState(StateObject stateObject)
    {
        await _daprClient.SaveStateAsync(_storeName, _stateKey, stateObject, metadata: _metadata);
    }

    private async Task deleteState()
    {
        await _daprClient.DeleteStateAsync(_storeName, _stateKey);

        // await daprClient.TryDeleteStateAsync(_storeName, _stateKey, etag);
    }

    private async Task<Results<Ok<ETagStateObject>, NoContent>> getStateAndETag()
    {
        // Examples: https://github.com/dapr/dotnet-sdk/blob/master/examples/Client/StateManagement/StateStoreETagsExample.cs

        (StateObject? stateObject, string etag) = await _daprClient.GetStateAndETagAsync<StateObject?>(_storeName, _stateKey);

        if (stateObject is null)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(new ETagStateObject(etag, stateObject));
    }

    private async Task<string> postStateWithETag(ETagStateObject etagState)
    {
        bool isSaved = await _daprClient.TrySaveStateAsync(_storeName, _stateKey, etagState.State, etagState.ETag, metadata: _metadata);

        return isSaved ? "Saved" : "ETag does not math!";
    }

    private sealed record StateObject(string Name, int Age);

    private sealed record ETagStateObject(string ETag, StateObject State);
}