using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace DaprWebApi.Endpoints.Common;

public interface IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder);
}

/* Usage
 * #1 -> services.AddEndpoints()
 * #2 -> app.MapEndpoints()
 */
public static class EndpointExtentions
{
    private readonly static Type _endpointType = typeof(IEndpoint);

    public static void AddEndpoints(this IServiceCollection services)
    {
        services.AddEndpoints(Assembly.GetExecutingAssembly());
    }

    public static void AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        // Note: Singleton dependencies can be injected into the endpoints class, like: DaprClient, ILogger

        ServiceDescriptor[] serviceDescriptors = assembly.DefinedTypes
            .Where(endpointPredicate)
            .Select(type => ServiceDescriptor.Singleton(_endpointType, type)) // Singleton or Transient does not matter. When you call MapEndpoints, the endpoint class is not disposed.
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        static bool endpointPredicate(Type type)
        {
            return type is { IsAbstract: false, IsInterface: false } &&
                   type.IsAssignableTo(_endpointType); // == _endpointType.IsAssignableFrom(type)
        }
    }

    public static void MapEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        IEnumerable<IEndpoint>? endpoints = routeBuilder.ServiceProvider.GetServices<IEndpoint>();

        if (endpoints is null || !endpoints.Any())
        {
            throw new InvalidOperationException("No IEndpoint is registered. Perhaps the services.AddEndpoints() method call is missing.");
        }

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoints(routeBuilder);
        }
    }
}
