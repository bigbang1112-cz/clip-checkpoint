using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GbxToolAPI.Client;

public abstract class ToolHubConnection : IAsyncDisposable
{    
    private readonly ILogger? logger;

    protected HubConnection Connection { get; }

    public bool Started { get; private set; }
    public string? ConnectionId => Connection.ConnectionId;
    public HubConnectionState State => Connection.State;

	public ToolHubConnection(string baseAddress, ILogger? logger = null)
    {
        this.logger = logger;

        if (!baseAddress.EndsWith('/'))
        {
            baseAddress += '/';
        }

        var type = GetType();

        var hubName = type.Name.ToLower();

        if (hubName.EndsWith("Connection", StringComparison.OrdinalIgnoreCase))
        {
            hubName = hubName[..^10];
        }

        var hubAddress = baseAddress + hubName;

        Connection = new HubConnectionBuilder()
            .WithUrl(hubAddress)
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();
    }

    public async Task<bool> StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Connection.StartAsync(cancellationToken);
            return Started = true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to start hub connection");
            return Started = false;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await Connection.StopAsync(cancellationToken);
        Started = false;
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}
