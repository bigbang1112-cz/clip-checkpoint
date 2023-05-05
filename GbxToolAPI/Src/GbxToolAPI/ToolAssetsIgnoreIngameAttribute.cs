namespace GbxToolAPI;

/// <summary>
/// Define a path in the assets folder to ignore from ingame assets.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ToolAssetsIgnoreIngameAttribute : Attribute
{
    public string Path { get; }

    public ToolAssetsIgnoreIngameAttribute(string path)
    {
        Path = path;
    }
}
