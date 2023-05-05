namespace GbxToolAPI.CLI.GameInstallations;

internal class Trackmania2020GameInstallation : GameInstallation
{
    public override string Name => Constants.Trackmania2020;
    public override string ExeName => "Trackmania";

    public override string[] SuggestedInstallationPaths { get; } = new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Ubisoft", "Ubisoft Game Launcher", "games", "Trackmania"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Ubisoft", "Ubisoft Game Launcher", "games", "Trackmania")
    };

    public override string? GetPathFromOptions(ConsoleOptions options)
    {
        return options.Trackmania2020InstallationPath;
    }

    public override void SetPathFromOptions(ConsoleOptions options, string? path)
    {
        options.Trackmania2020InstallationPath = path;
    }
}
