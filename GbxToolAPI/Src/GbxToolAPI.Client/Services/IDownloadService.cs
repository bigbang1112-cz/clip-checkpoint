namespace GbxToolAPI.Client.Services;

public interface IDownloadService
{
    Task DownloadAsync(string fileName, object content, string mimeType);
}
