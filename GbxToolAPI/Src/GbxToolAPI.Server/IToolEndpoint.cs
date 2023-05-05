using Microsoft.AspNetCore.Routing;

namespace GbxToolAPI.Server;

public interface IToolEndpoint
{
    void Endpoint(IEndpointRouteBuilder app);
}
