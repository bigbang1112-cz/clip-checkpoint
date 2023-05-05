namespace GbxToolAPI;

[AttributeUsage(AttributeTargets.Class)]
public class ToolDescriptionAttribute : Attribute
{
    public string Description { get; }

    public ToolDescriptionAttribute(string description)
    {
        Description = description;
    }
}
