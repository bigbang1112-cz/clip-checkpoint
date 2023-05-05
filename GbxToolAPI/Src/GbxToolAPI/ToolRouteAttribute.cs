namespace GbxToolAPI;

[AttributeUsage(AttributeTargets.Class)]
public class ToolRouteAttribute : Attribute
{
    public string Route { get; }

    public ToolRouteAttribute(string route)
	{
        Route = route;
    }
}
