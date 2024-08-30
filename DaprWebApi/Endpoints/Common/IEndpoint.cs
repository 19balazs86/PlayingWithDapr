namespace DaprWebApi.Endpoints.Common;

public interface IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder routeBuilder);
}
