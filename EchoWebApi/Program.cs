namespace EchoWebApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        WebApplication app = builder.Build();

        app.MapFallback(handelAllRequests);

        app.Run();
    }

    private static async Task<EchoResponse> handelAllRequests(HttpContext context, ILogger<EchoResponse> logger, CancellationToken ct)
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

        // This log message will appear in the Output window from Dapr
        logger.LogInformation("Handle {method}:{path}", response.Method, response.Path);

        return response;
    }
}

public sealed class EchoResponse
{
    public required string Method { get; init; }

    public required string Path { get; init; }

    public required string QueryString { get; init; }

    public required string Body { get; init; }
}