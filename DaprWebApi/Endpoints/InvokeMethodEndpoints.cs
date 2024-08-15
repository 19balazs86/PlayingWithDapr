using Dapr.Client;
using System.Collections.Immutable;

namespace DaprWebApi.Endpoints;

// Examples:
// - HTTP, gRPC, HttpClient: https://github.com/dapr/dotnet-sdk/tree/master/examples/Client/ServiceInvocation
public sealed class InvokeMethodEndpoints : IEndpoint
{
    private static readonly ImmutableList<HttpMethod> _httpMethods = [HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete];

    private const string _appId = "echo-server";

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // https://github.com/dapr/dotnet-sdk/blob/master/examples/Client/ServiceInvocation/InvokeServiceHttpClientExample.cs
        // You can create HttpClient to call the EchoWebApi endpoints
        // HttpClient httpClient = DaprClient.CreateInvokeHttpClient(_appId);

        app.MapGet("/invoke-method", invokeEchoServer);
    }

    private static async Task<EchoResponse> invokeEchoServer(DaprClient daprClient, CancellationToken ct)
    {
        HttpMethod httpMethod = _httpMethods[Random.Shared.Next(0, _httpMethods.Count)];

        EchoResponse response;

        if (httpMethod.Method is "GET" or "DELETE")
        {
            response = await daprClient.InvokeMethodAsync<EchoResponse>(httpMethod, _appId, "get-delete-endpoint?key=value", ct);
        }
        else // POST or PUT
        {
            var request = new EchoRequest("John Doe", 10);

            response = await daprClient.InvokeMethodAsync<EchoRequest, EchoResponse>(httpMethod, _appId, "post-put-endpoint?key=value", request, ct);
        }

        return response;
    }

    private sealed record EchoRequest(string Name, int Age);

    private sealed record EchoResponse(string Method, string Path, string QueryString, string Body);
}
