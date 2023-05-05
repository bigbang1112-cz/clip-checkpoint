namespace GbxToolAPI.CLI;

internal abstract class GameInstallation
{
    public abstract string Name { get; }
    public abstract string ExeName { get; }
    public abstract string[] SuggestedInstallationPaths { get; }
    public abstract string? GetPathFromOptions(ConsoleOptions options);
    public abstract void SetPathFromOptions(ConsoleOptions options, string? path);
}
