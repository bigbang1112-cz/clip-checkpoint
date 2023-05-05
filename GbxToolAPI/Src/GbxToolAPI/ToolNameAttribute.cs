namespace GbxToolAPI;

[AttributeUsage(AttributeTargets.Class)]
public class ToolNameAttribute : Attribute
{
    public string Name { get; }

    public ToolNameAttribute(string name)
    {
        Name = name;
    }
}
