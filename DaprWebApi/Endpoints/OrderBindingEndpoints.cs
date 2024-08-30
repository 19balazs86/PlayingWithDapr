using Dapr.Client;
using DaprWebApi.DTOs;
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
        group.MapPost("/checkout", checkout);
    }

    // For simplicity, input and output binding within the same application
    private static async Task invoke(DaprClient daprClient)
    {
        var order = Order.CreateNew();

        await daprClient.InvokeBindingAsync(_bindingName, "create", order);
    }

    private static StatusCodeHttpResult checkout(Order order, ILogger<OrderBindingEndpoints> logger)
    {
        /* To ENSURE the incoming request is initiated by the Dapr sidecar, you can check
         * - Request.Headers["dapr-api-token"] == Environment.GetEnvironmentVariable("APP_API_TOKEN")
         *
         * Setup in Self-Hosted mode
         * - You need to set the APP_API_TOKEN environment variable
         * - Dapr sidecar will include this token in all requests sent to your app
         * - Documentation: https://docs.dapr.io/operations/security/app-api-token
         *
         * - Azure Container App: https://learn.microsoft.com/en-us/azure/container-apps/dapr-authentication-token
         */

        // Simulate server error. Dapr will resend the storage queue message with visibilityTimeout
        int statusCode = Random.Shared.NextDouble() <= 0.8 ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

        logger.LogInformation("Checkout-Binding for Order #{id} | CreatedAt: {date} | StatusCode {code}", order.Id, order.CreatedAtUtc, statusCode);

        return TypedResults.StatusCode(statusCode);
    }
}