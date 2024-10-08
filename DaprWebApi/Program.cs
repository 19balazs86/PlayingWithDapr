using DaprWebApi.Endpoints.Common;

namespace DaprWebApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IServiceCollection services = builder.Services;

        // Add services to the container
        {
            services.AddDaprClient();

            services.AddEndpoints(); // From: EndpointExtentions
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.MapGet("/", () => "Hello DaprWebApi");

            // This middleware unwraps any request body with a CloudEvents content type
            // Allowing the HTTP endpoint handler to obtain the desired JSON object
            app.UseCloudEvents();

            // Map example endpoints
            app.MapEndpoints(); // From: EndpointExtentions
        }

        app.Run();
    }
}
