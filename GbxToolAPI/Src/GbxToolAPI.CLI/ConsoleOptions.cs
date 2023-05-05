namespace GbxToolAPI.CLI;

public class ConsoleOptions
{
    public bool NoPause { get; set; }
    public bool SingleOutput { get; set; }
    public string? CustomConfig { get; set; }
    public string? OutputDir { get; set; }
    public string? TrackmaniaForeverInstallationPath { get; set; }
    public string? ManiaPlanetInstallationPath { get; set; }
    public string? TrackmaniaTurboInstallationPath { get; set; }
    public string? Trackmania2020InstallationPath { get; set; }
}
