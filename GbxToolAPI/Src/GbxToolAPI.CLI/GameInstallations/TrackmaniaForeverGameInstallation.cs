namespace GbxToolAPI.CLI.GameInstallations;

internal class TrackmaniaForeverGameInstallation : GameInstallation
{
    public override string Name => Constants.TrackManiaForever;
    public override string ExeName => "TmForever";

    public override string[] SuggestedInstallationPaths { get; } = new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TmUnitedForever"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "TmUnitedForever"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "TrackMania United"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "TrackMania United"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "TmNationsForever"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "TmNationsForever"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "TrackMania Nations Forever"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "TrackMania Nations Forever"),
    };

    public override string? GetPathFromOptions(ConsoleOptions options)
    {
        return options.TrackmaniaForeverInstallationPath;
    }

    public override void SetPathFromOptions(ConsoleOptions options, string? path)
    {
        options.TrackmaniaForeverInstallationPath = path;
    }
}
