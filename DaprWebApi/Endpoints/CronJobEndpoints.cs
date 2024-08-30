namespace DaprWebApi.Endpoints;

// Cron binding spec: https://docs.dapr.io/reference/components-reference/supported-bindings/cron

// There is also a Job scheduling API, currently in alpha, I could not find any related methods in DaprClient
// https://docs.dapr.io/developing-applications/building-blocks/jobs/jobs-overview
// Job introduction with HTTP calls: https://youtu.be/1Mnl7Dlo6Bo?t=120
public sealed class CronJobEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Endpoint matches the name of the component, defined in: cron-job-binding.yaml
        app.MapPost("/my-cron-job", handleJob);
    }

    private static void handleJob(ILogger<CronJobEndpoints> logger)
    {
        // Request.Body is empty

        // To ENSURE the incoming request is initiated by the Dapr sidecar: read the checkout method in OrderBindingEndpoints.cs

        logger.LogInformation("my-cron-job is triggered at: {time}", DateTime.Now.ToLongTimeString());
    }
}