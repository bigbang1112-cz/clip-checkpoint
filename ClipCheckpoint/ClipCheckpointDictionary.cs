using GbxToolAPI;
using YamlDotNet.Serialization;

namespace ClipCheckpoint;

public class ClipCheckpointDictionary : ITextDictionary
{
    [YamlMember(Description = "Name of the checkpoint time text track.")]
    public string MediaTrackerTrackCheckpointTime { get; set; } = "CC: Time";

    [YamlMember(Description = "Name of the checkpoint time shadow text track.")]
    public string MediaTrackerTrackCheckpointTimeShadow { get; set; } = "CC: Time (shadow)";

    [YamlMember(Description = "Name of the checkpoint sound track.")]
    public string MediaTrackerTrackCheckpointSound { get; set; } = "CC: Sound";

    [YamlMember(Description = "Name of the checkpoint lap text track.")]
    public string MediaTrackerTrackCheckpointLap { get; set; } = "CC: Lap";

    [YamlMember(Description = "Name of the checkpoint delta text track.")]
    public string MediaTrackerTrackCheckpointDelta { get; set; } = "CC: Delta";
}
