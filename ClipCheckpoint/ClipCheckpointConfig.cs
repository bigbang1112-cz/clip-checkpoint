using GBX.NET;
using YamlDotNet.Serialization;

namespace BigBang1112.ClipCheckpoint;

public class ClipCheckpointConfig
{
    [YamlMember(Description = "Aspect ratio used to space out and scale elements. Unless you have a different monitor aspect ratio, 16:9 works the best universally.")]
    public Vec2 AspectRatio { get; set; } = (16, 9);

    [YamlMember(Description = "Scale of the checkpoint display.")]
    public Vec2 Scale { get; set; } = (1, 1);

    [YamlMember(Description = "Position of the checkpoint display.")]
    public Vec2 Position { get; set; } = (0, 0.5f);

    [YamlMember(Description = "Depth (Z distance) of the checkpoint display.")]
    public float Depth { get; set; } = 0.5f;

    [YamlMember(Description = "Max length of each checkpoint display in seconds.")]
    public TimeSpan Length { get; set; } = TimeSpan.FromSeconds(2);

    [YamlMember(Description = "Volume of the checkpoint sound.")]
    public float Volume { get; set; } = 1;

    [YamlMember(Description = "Color of the main checkpoint time text.")]
    public Vec3 Color { get; set; } = (1, 1, 1);

    [YamlMember(Description = "Color of the shadow of the main checkpoint time text.")]
    public Vec3 ShadowColor { get; set; } = (0, 0, 0);

    [YamlMember(Description = "Height (distance) of the shadow of the main checkpoint time text.")]
    public Vec2 ShadowHeight { get; set; } = (0.005f, 0.005f);

    [YamlMember(Description = "Depth (Z distance) offset of the shadow of the main checkpoint time text (added on Depth).")]
    public float ShadowDepthOffset { get; set; } = 0.5f;

    [YamlMember(Description = "Position offset of the lap time text (added on Position).")]
    public Vec2 LapTimePositionOffset { get; set; } = (0, 0.1f);

    [YamlMember(Description = "Scale of the lap time text (multiplied on Scale).")]
    public Vec2 LapTimeScale { get; set; } = (0.75f, 0.75f);

    [YamlMember(Description = "Color of the main checkpoint time.")]
    public Vec3 LapTimeColor { get; set; } = (1, 1, 0);

    [YamlMember(Description = "Position offset of the delta time text (added on Position).")]
    public Vec2 DeltaTimePositionOffset { get; set; } = (0, -0.1f);

    [YamlMember(Description = "Scale of the delta time text (multiplied on Scale).")]
    public Vec2 DeltaTimeScale { get; set; } = (0.75f, 0.75f);

    [YamlMember(Description = "Color of the delta minus checkpoint time text.")]
    public Vec3 DeltaNegativeColor { get; set; } = (0, 0, 1);

    [YamlMember(Description = "Color of the delta equal checkpoint time text.")]
    public Vec3 DeltaEqualColor { get; set; } = (1, 0, 1);

    [YamlMember(Description = "Color of the delta plus checkpoint time text.")]
    public Vec3 DeltaPositiveColor { get; set; } = (1, 0, 0);

    [YamlMember(Description = "Length of the start of the animation.")]
    public TimeSpan AnimationInLength { get; set; } = TimeSpan.FromSeconds(0.1);

    [YamlMember(Description = "Length of the end of the animation.")]
    public TimeSpan AnimationOutLength { get; set; } = TimeSpan.FromSeconds(0.2);

    [YamlMember(Description = "Starting scale of the animation.")]
    public Vec2 AnimationInScale { get; set; } = (0.9f, 0.9f);

    [YamlMember(Description = "Ending scale of the animation.")]
    public Vec2 AnimationOutScale { get; set; } = (1, 1);

    [YamlMember(Description = "If the main checkpoint text should be included on finish (finish is also taken as checkpoint).")]
    public bool IncludeFinish { get; set; } = false;

    [YamlMember(Description = "If the sound should be included.")]
    public bool IncludeSound { get; set; } = true;

    [YamlMember(Description = "Name of the checkpoint time text track.")]
    public string TrackNameCheckpointTime { get; set; } = "CP TIME";

    [YamlMember(Description = "Name of the checkpoint time shadow text track.")]
    public string TrackNameCheckpointTimeShadow { get; set; } = "CP TIME SH";

    [YamlMember(Description = "Name of the checkpoint sound track.")]
    public string TrackNameCheckpointSound { get; set; } = "CP SOUND";

    [YamlMember(Description = "Name of the checkpoint lap text track.")]
    public string TrackNameCheckpointLap { get; set; } = "CP LAP";

    [YamlMember(Description = "Name of the checkpoint delta text track.")]
    public string TrackNameCheckpointDelta { get; set; } = "CP DELTA";

    [YamlMember(Description = "Format of the main checkpoint time text. {0} is the time (X:XX.XXX or X:XX.XX)")]
    public string TextCheckpointFormat { get; set; } = "$o$n{0}";
    
    [YamlMember(Description = "Format of the lap time text. {0} is the lap time (X:XX.XXX or X:XX.XX)")]
    public string TextLapFormat { get; set; } = "$o$n$s{0}";

    [YamlMember(Description = "Format of the delta time text. {0} is the delta time ((+)X:XX.XXX or (+)X:XX.XX)")]
    public string TextDeltaFormat { get; set; } = "$o$n$s{0}";

    [YamlMember(Description = "Format of the stunts score addition to the main checkpoint time text. {0} is the stunts score.")]
    public string TextStuntsFormat { get; set; } = "({0} pts.)";

    [YamlMember(Description = "URL location of the regular checkpoint sound (must be ogg).")]
    public string CheckpointSoundUrl { get; set; } = "https://bigbang1112.cz/temp/RaceCheckPoint.ogg";

    [YamlMember(Description = "URL location of the lap sound (must be ogg).")]
    public string LapSoundUrl { get; set; } = "https://bigbang1112.cz/temp/RaceLap.ogg";

    [YamlMember(Description = "URL location of the Snow environment checkpoint sound (must be ogg).")]
    public string AlpineCheckpointSoundUrl { get; set; } = "https://bigbang1112.cz/temp/AlpineCheckPoint.ogg";

    [YamlMember(Description = "URL location of the Rally environment checkpoint sound (must be ogg).")]
    public string RallyCheckpointSoundUrl { get; set; } = "https://bigbang1112.cz/temp/RallyCheckPoint.ogg";

    [YamlMember(Description = "URL location of the Desert environment checkpoint sound (must be ogg).")]
    public string SpeedCheckpointSoundUrl { get; set; } = "https://bigbang1112.cz/temp/SpeedCheckPoint.ogg";

    [YamlMember(Description = "Allows support for TMU and TMS but limits support for TM2 and TM2020. TMUF support isn't affected.")]
    public bool Legacy { get; set; } = false;
}
