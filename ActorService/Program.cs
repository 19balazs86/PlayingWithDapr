using ActorService.CounterFeature;
using System.Net.Mime;

namespace ActorService;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IServiceCollection services   = builder.Services;

        // Add services to the container
        {
            services.AddActors(options =>
            {
                // Configuration: https://docs.dapr.io/developing-applications/building-blocks/actors/actors-runtime-config

                options.Actors.RegisterActor<CounterActor>();
            });
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.MapGet("/", () => TypedResults.Content("Hello from Actor service | <a href='/counter/add'>Add counter</a>", MediaTypeNames.Text.Html));

            app.MapActorsHandlers();

            app.MapCounterEndpoints();
        }

        app.Run();
    }
}
