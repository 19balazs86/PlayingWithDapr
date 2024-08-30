using Dapr.Client;
using DaprWebApi.DTOs;
using DaprWebApi.Endpoints.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DaprWebApi.Endpoints;

public sealed class OrderBindingEndpoints : IEndpoint
{
    private const string _bindingName = "order-binding";

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Endpoint based on the route in binding.yaml
        var group = app.MapGroup("/order-binding");

        group.MapGet("/invoke",    invoke);
        group.MapPost("/checkout", checkout).AddEndpointFilter<DaprApiTokenEndpointFilter>();
    }

    // For simplicity, input and output binding within the same application
    private static async Task invoke(DaprClient daprClient)
    {
        var order = Order.CreateNew();

        await daprClient.InvokeBindingAsync(_bindingName, "create", order);
    }

    private static StatusCodeHttpResult checkout(Order order, ILogger<OrderBindingEndpoints> logger)
    {
        // To ENSURE the incoming request is initiated by Dapr use the DaprApiTokenEndpointFilter

        // Note: Request.Headers contains information about the message

        // Simulate server error. Dapr will resend the storage queue message with visibilityTimeout
        int statusCode = Random.Shared.NextDouble() <= 0.8 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

        logger.LogInformation("Checkout-Binding for Order #{id} | CreatedAt: {date} | StatusCode {code}", order.Id, order.CreatedAtUtc, statusCode);

        return TypedResults.StatusCode(statusCode);
    }
}