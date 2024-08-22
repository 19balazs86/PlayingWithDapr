using JobWorker.Workers;

namespace JobWorker;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var services = builder.Services;

        // Add services to the container
        {
            services.AddHostedService<Worker>();
        }

        IHost host = builder.Build();

        host.Run();
    }
}