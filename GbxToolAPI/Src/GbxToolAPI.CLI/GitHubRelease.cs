using System.Text.Json.Serialization;

namespace GbxToolAPI.CLI;

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public required string TagName { get; set; }

    [JsonPropertyName("published_at")]
    public required DateTimeOffset PublishedAt { get; set; }
}
