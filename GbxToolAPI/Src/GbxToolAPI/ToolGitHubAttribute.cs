namespace GbxToolAPI;

[AttributeUsage(AttributeTargets.Class)]
public class ToolGitHubAttribute : Attribute
{
    public string Repository { get; }
    public bool NoExe { get; set; }

    public ToolGitHubAttribute(string repository)
    {
        Repository = repository;
    }
}
