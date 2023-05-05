namespace GbxToolAPI;

[AttributeUsage(AttributeTargets.Class)]
public class ToolAssetsAttribute : Attribute
{
    public string Identifier { get; }

    public ToolAssetsAttribute(string identifier)
	{
        Identifier = identifier;
    }
}
