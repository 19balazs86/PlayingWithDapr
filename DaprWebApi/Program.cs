using DaprWebApi.Endpoints;

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
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.MapGet("/", () => "Hello DaprWebApi");

            app.MapStateEndpoints();
            app.MapInvokeMethodEndpoints();
        }

        app.Run();
    }
}
