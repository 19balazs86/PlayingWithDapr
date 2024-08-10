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

    public static void MapStateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/state");

        group.MapGet("/",    stateGet);
        group.MapPost("/",   statePost);
        group.MapDelete("/", stateDelete);
    }

    private static async Task<Results<Ok<StateObject>, NoContent>> stateGet(DaprClient daprClient)
    {
        StateObject? stateObject = await daprClient.GetStateAsync<StateObject?>(_storeName, _stateKey);

        if (stateObject is null)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(stateObject);
    }

    private static async Task statePost(StateObject stateObject, DaprClient daprClient)
    {
        var metadata = new Dictionary<string, string>
        {
            ["ttlInSeconds"] = "120" // TTL set to 2 minutes
        };

        await daprClient.SaveStateAsync(_storeName, _stateKey, stateObject, metadata: metadata);
    }

    private static async Task stateDelete(DaprClient daprClient)
    {
        await daprClient.DeleteStateAsync(_storeName, _stateKey);
    }
}

public sealed record StateObject(string Name, int Age);