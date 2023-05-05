namespace GbxToolAPI.Server;

[AttributeUsage(AttributeTargets.Assembly)]
public class ToolEndpointAttribute : Attribute
{
    public string Route { get; }

    public ToolEndpointAttribute(string route)
	{
        Route = route;
    }
}
