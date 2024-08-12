using Dapr.Client;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DaprWebApi.Endpoints;

// Examples
// - HTTP and SDK: https://github.com/dapr/quickstarts/tree/master/state_management/csharp
// - Transactions, ETags, Bulk: https://github.com/dapr/dotnet-sdk/tree/master/examples/Client/StateManagement
public static class StateEndpoints
{
    private const string _storeName = "MyStateStore";
    private const string _stateKey  = "test-key";

    private static readonly Dictionary<string, string> _metadata = new() { ["ttlInSeconds"] = "120" }; // TTL set to 2 minutes

    public static void MapStateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/state");

        group.MapGet("/",    getState);
        group.MapPost("/",   postState);
        group.MapDelete("/", deleteState);

        group.MapGet("/with-etag",  getStateAndETag);
        group.MapPost("/with-etag", postStateWithETag);
    }

    private static async Task<Results<Ok<StateObject>, NoContent>> getState(DaprClient daprClient)
    {
        StateObject? stateObject = await daprClient.GetStateAsync<StateObject?>(_storeName, _stateKey);

        if (stateObject is null)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(stateObject);
    }

    private static async Task postState(StateObject stateObject, DaprClient daprClient)
    {
        await daprClient.SaveStateAsync(_storeName, _stateKey, stateObject, metadata: _metadata);
    }

    private static async Task deleteState(DaprClient daprClient)
    {
        await daprClient.DeleteStateAsync(_storeName, _stateKey);

        // await daprClient.TryDeleteStateAsync(_storeName, _stateKey, etag);
    }

    private static async Task<Results<Ok<ETagStateObject>, NoContent>> getStateAndETag(DaprClient daprClient)
    {
        // Examples: https://github.com/dapr/dotnet-sdk/blob/master/examples/Client/StateManagement/StateStoreETagsExample.cs

        (StateObject? stateObject, string etag) = await daprClient.GetStateAndETagAsync<StateObject?>(_storeName, _stateKey);

        if (stateObject is null)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(new ETagStateObject(etag, stateObject));
    }

    private static async Task<string> postStateWithETag(ETagStateObject etagState, DaprClient daprClient)
    {
        bool isSaved = await daprClient.TrySaveStateAsync(_storeName, _stateKey, etagState.State, etagState.ETag, metadata: _metadata);

        return isSaved ? "Saved" : "ETag does not math!";
    }

    private sealed record StateObject(string Name, int Age);

    private sealed record ETagStateObject(string ETag, StateObject State);
}