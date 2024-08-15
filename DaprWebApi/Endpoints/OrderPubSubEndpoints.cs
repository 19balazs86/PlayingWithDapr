using Dapr.Client;
using DaprWebApi.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DaprWebApi.Endpoints;

// Example: https://github.com/dapr/quickstarts/tree/master/pub_sub/csharp/sdk
public sealed class OrderPubSubEndpoints : IEndpoint
{
    private const string _pubSubName = "my-pub-sub";
    private const string _topicName  = "orders";

    private readonly DaprClient _daprClient;
    private readonly ILogger<OrderPubSubEndpoints> _logger;

    public OrderPubSubEndpoints(DaprClient daprClient, ILogger<OrderPubSubEndpoints> logger)
    {
        _daprClient = daprClient;
        _logger     = logger;
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Endpoints based on the path in subscription.yaml
        var group = app.MapGroup("/orders-pub-sub");

        group.MapGet("/publish",   publish);
        group.MapPost("/checkout", checkout);
    }

    // For simplicity, publish and handle messages within the same application.
    // In production, other applications can access the pub-sub, publish messages, and subscribe to the topic.
    private async Task publish()
    {
        var order = Order.CreateNew();

        await _daprClient.PublishEventAsync(_pubSubName, _topicName, order);
    }

    // Declarative and programmatic subscription methods
    // https://docs.dapr.io/developing-applications/building-blocks/pubsub/subscription-methods
    private StatusCodeHttpResult checkout(Order order)
    {
        // Do not forget: app.UseCloudEvents()

        // Simulate server error, and Dapr will retry the message by default
        // Retries and dead-letter: https://docs.dapr.io/developing-applications/building-blocks/pubsub/pubsub-deadletter
        int statusCode = Random.Shared.NextDouble() <= 0.8 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

        _logger.LogInformation("Checkout-PubSub for Order #{id} | CreatedAt: {date} | StatusCode {code}", order.Id, order.CreatedAtUtc, statusCode);

        return TypedResults.StatusCode(statusCode);
    }
}