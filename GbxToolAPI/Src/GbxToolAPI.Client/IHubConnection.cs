namespace GbxToolAPI.Client;

public interface IHubConnection<T> where T : ToolHubConnection
{
    public T HubConnection { get; init; }
}
