using System.Collections.Immutable;

namespace EchoWebApi;

public static class Program
{
    private static readonly ImmutableList<string> _catchAllHttpMethods = ["GET", "POST", "PUT", "DELETE"];

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IServiceCollection services = builder.Services;

        // Add services to the container
        {

        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.UseHttpsRedirection();

            app.MapMethods("/{**catchAll}", _catchAllHttpMethods, catchAll);
        }

        app.Run();
    }

    private static async Task catchAll(HttpContext context, CancellationToken ct)
    {
        await Task.Delay(2_000, ct);

        HttpRequest request = context.Request;

        using var streamReader = new StreamReader(request.Body);

        string body = await streamReader.ReadToEndAsync();

        var response = new EchoResponse
        {
            //Headers     = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Method      = request.Method,
            Path        = request.Path.ToString(),
            QueryString = request.QueryString.ToString(),
            Body        = body
        };

        HttpResponse httpResponse = context.Response;

        await httpResponse.WriteAsJsonAsync(response);
    }
}

public sealed class EchoResponse
{
    public required string Method { get; init; }

    public required string Path { get; init; }

    public required string QueryString { get; init; }

    public required string Body { get; init; }
}