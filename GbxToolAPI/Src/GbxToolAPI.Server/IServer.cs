using Microsoft.Extensions.DependencyInjection;

namespace GbxToolAPI.Server;

public interface IServer
{
    string ConnectionString { get; }
    static abstract void Services(IServiceCollection services);
}
