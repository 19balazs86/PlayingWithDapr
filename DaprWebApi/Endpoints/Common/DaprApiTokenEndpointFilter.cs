namespace DaprWebApi.Endpoints.Common;

/// <summary>
/// Usage: app.MapPost("/endpoint", ...).AddEndpointFilter<DaprApiTokenEndpointFilter>();
/// </summary>
public sealed class DaprApiTokenEndpointFilter : IEndpointFilter
{
    /* To ENSURE the incoming request is initiated by the Dapr sidecar, you can check
     * - Request.Headers["Dapr-Api-Token"] == Environment.GetEnvironmentVariable("APP_API_TOKEN")
     *
     * Setup in Self-Hosted mode
     * - You need to set the APP_API_TOKEN environment variable
     * - Dapr sidecar will include this token in all requests sent to your app
     * - Documentation: https://docs.dapr.io/operations/security/app-api-token
     *
     * - Azure Container App: https://learn.microsoft.com/en-us/azure/container-apps/dapr-authentication-token
     */
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        string? envApiToken    = Environment.GetEnvironmentVariable("APP_API_TOKEN");
        string? headerApiToken = context.HttpContext.Request.Headers["Dapr-Api-Token"];

        if (envApiToken == headerApiToken)
        {
            return await next(context);
        }

        return TypedResults.Unauthorized();
    }
}