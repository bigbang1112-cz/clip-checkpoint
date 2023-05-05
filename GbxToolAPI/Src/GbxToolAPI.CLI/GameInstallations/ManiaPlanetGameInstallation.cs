namespace GbxToolAPI.CLI.GameInstallations;

internal class ManiaPlanetGameInstallation : GameInstallation
{
    public override string Name => Constants.ManiaPlanet;
    public override string ExeName => Constants.ManiaPlanet;

    public override string[] SuggestedInstallationPaths { get; } = new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ManiaPlanet"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ManiaPlanet"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "ManiaPlanet_TMStadium"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "ManiaPlanet_TMStadium"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "ManiaPlanet_TMCanyon"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "ManiaPlanet_TMCanyon"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "ManiaPlanet_TMValley"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "ManiaPlanet_TMValley"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "ManiaPlanet_TMLagoon"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "ManiaPlanet_TMLagoon")
    };

    public override string? GetPathFromOptions(ConsoleOptions options)
    {
        return options.ManiaPlanetInstallationPath;
    }

    public override void SetPathFromOptions(ConsoleOptions options, string? path)
    {
        options.ManiaPlanetInstallationPath = path;
    }
}
