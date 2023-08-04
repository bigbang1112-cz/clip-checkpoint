using ClipCheckpoint.Exceptions;
using ClipCheckpoint.Externsions;
using GBX.NET;
using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using GbxToolAPI;
using System.Diagnostics;
using System.Xml;
using TmEssentials;

namespace ClipCheckpoint;

[ToolName("Clip Checkpoint")]
[ToolDescription("Transfers checkpoint data from a replay/ghost into a MediaTracker clip.")]
[ToolGitHub("bigbang1112-cz/clip-checkpoint")]
[ToolAssets("ClipCheckpoint")]
public class ClipCheckpointTool : ITool, IHasOutput<NodeFile<CGameCtnMediaClip>>, IHasAssets, IConfigurable<ClipCheckpointConfig>
{
    private readonly CGameCtnChallenge? map;
    private readonly IEnumerable<CGameCtnReplayRecord> replays;
    private readonly IEnumerable<CGameCtnGhost> ghosts;

    public ClipCheckpointConfig Config { get; set; } = new();

    public ClipCheckpointTool(CGameCtnReplayRecord replay) : this(replay.GetGhosts())
    {
        replays = Enumerable.Repeat(replay, 1);
        map = replay.Challenge;
    }

    public ClipCheckpointTool(IEnumerable<CGameCtnReplayRecord> replays) : this(replays.SelectMany(x => x.GetGhosts().Take(1)))
    {
        this.replays = replays;
        map = replays.First().Challenge;
    }

    public ClipCheckpointTool(CGameCtnGhost ghost) : this(Enumerable.Repeat(ghost, 1))
    {
        
    }

    public ClipCheckpointTool(IEnumerable<CGameCtnGhost> ghosts)
    {
        this.ghosts = ghosts;
        replays = Enumerable.Empty<CGameCtnReplayRecord>();
    }

    public ValueTask LoadAssetsAsync()
    {
        return ValueTask.CompletedTask;
    }

    public static string RemapAssetRoute(string route, bool isManiaPlanet)
    {
        if (route.StartsWith("Sounds"))
        {
            return Path.Combine("Media" + (isManiaPlanet ? "" : "Tracker"), "Sounds", "ClipCheckpoint", route[("Images".Length + 1)..]);
        }

        return "";
    }

    public NodeFile<CGameCtnMediaClip> Produce()
    {
        var ghost = ghosts.First();
        var deltaGhost = ghosts.Skip(1).FirstOrDefault();

        //Console.Write("Extracting number of laps... ");
        var laps = GetNumberOfLaps(ghost.Validate_RaceSettings);

        var isMultilap = laps > 1;
        //Console.WriteLine(isMultilap ? "Multiple laps ({0})" : "No laps", laps);

        var isFromTM2 = GameVersion.IsManiaPlanet(ghost);
        //Console.WriteLine(isFromTM2 ? "Ghost is from TM2 or higher version." : "Ghost is from TMUF or lower version.");

        var checkpointSoundFileRef = default(FileRef);
        var lapSoundFileRef = default(FileRef);

        if (Config.IncludeSound)
        {
            //Console.Write("Creating the checkpoint sound file reference... ");
            checkpointSoundFileRef = CreateCheckpointSoundFileRef();
            //Console.WriteLine("Done");

            //Console.Write("Creating the lap sound file reference... ");
            lapSoundFileRef = CreateLapSoundFileRef();
            //Console.WriteLine("Done");
        }

        var checkpoints = ghost.Checkpoints;

        if (checkpoints is null || checkpoints.Length == 0)
            throw new NoCheckpointsException();

        var otherGhostCpTimes = deltaGhost?.Checkpoints?
            .Select(x => x.Time ?? throw new CheckpointIsMinusOneException())
            .ToArray();

        //Console.WriteLine("Checkpoints extracted.");

        // 'Real' CP count depending if also shown on finish line or not
        var checkpointCount = Config.IncludeFinish ? checkpoints.Length : checkpoints.Length - 1;
        //Console.WriteLine("Checkpoint count: " + checkpointCount);

        var checkpointCountPerLap = checkpoints.Length / laps;
        //Console.WriteLine("Checkpoint count per lap: " + checkpointCountPerLap);

        var textMediaBlocks = new CGameCtnMediaBlockText[checkpointCount];
        var textShadowMediaBlocks = new CGameCtnMediaBlockText[checkpointCount]; // TODO: layering
        var textMultilapMediaBlocks = new CGameCtnMediaBlockText[checkpoints.Length - checkpointCountPerLap];
        var textDeltaMediaBlocks = new CGameCtnMediaBlockText[deltaGhost is null ? 0 : checkpoints.Length];
        var soundMediaBlocks = new CGameCtnMediaBlockSound[Config.IncludeSound ? checkpointCount : 0];

        var textMultilapCrossMediaBlocks = new CGameCtnMediaBlockText[laps > 0 ? laps - 1 : 0];

        for (var i = 0; i < checkpoints.Length; i++)
        {
            //Console.WriteLine("Checkpoint {0}:", i + 1);

            var cp = checkpoints[i];

            if (!cp.Time.TryValue(out TimeInt32 time))
            {
                if (deltaGhost is null)
                {
                    //Console.WriteLine("-> -1 time??? Skipping.");
                    continue;
                }

                throw new CheckpointIsMinusOneException();
            }

            if (isMultilap)
            {
                if (Config.IncludeLapCross && (i + 1) % checkpointCountPerLap == 0)
                {
                    var lapNumber = (i + 1) / checkpointCountPerLap + 1;
                    //Console.WriteLine("-> Lap checkpoint.");

                    //Console.Write("-> Creating lap text media block... ");
                    if (lapNumber <= laps)
                    {
                        textMultilapCrossMediaBlocks[lapNumber - 2] = CreateLapCrossMediaBlock(time, lapNumber, laps);
                    }
                }

                // Lap MT blocks - if lap race - and an unique lap time was reached
                if (i >= checkpointCountPerLap)
                {
                    // Index to use for assinging to the lap MT block array
                    var index = i - checkpointCountPerLap;

                    //Console.Write("-> Creating lap text media block... ");
                    textMultilapMediaBlocks[index] = CreateLapMediaBlock(
                        time,
                        checkpoints,
                        currentCheckpointIndex: i,
                        checkpointCountPerLap,
                        isFromTM2);
                    //Console.WriteLine("Done");
                }
            }

            if (deltaGhost is not null)
            {
                var otherGhostTime = otherGhostCpTimes![i];

                // Calculate interval 
                var interval = cp.Time.GetValueOrDefault() - otherGhostTime;

                textDeltaMediaBlocks[i] = CreateDeltaMediaBlock(time, isFromTM2, interval);
            }

            // If the REAL checkpoint count was reached, ignore the rest of the checkpoints array
            if (i == checkpointCount)
            {
                //Console.WriteLine("Skipping the finish checkpoint time.");
                break;
            }

            // Actual checkpoint MT blocks

            var timeStr = cp.Time.ToTmString(useHundredths: !isFromTM2);
            var timeText = string.Format(Config.TextCheckpointFormat, timeStr);

            if (map?.Mode == CGameCtnChallenge.PlayMode.Stunts)
            {
                timeText += " " + string.Format(Config.TextStuntsFormat, cp.StuntsScore);
            }

            //Console.Write("-> Creating checkpoint text media block ({0})... ", timeStr);
            textMediaBlocks[i] = CreateCheckpointTextMediaBlock(time,
                timeText,
                color: Config.Color);
            //Console.WriteLine("Done");

            //Console.Write("-> Creating checkpoint text shadow media block... ");
            textShadowMediaBlocks[i] = CreateCheckpointTextMediaBlock(time,
                timeText,
                offsetPosition: -Config.ShadowHeight,
                offsetDepth: Config.ShadowDepthOffset,
                color: Config.ShadowColor);
            //Console.WriteLine("Done");

            if (!Config.IncludeSound)
                continue;

            var soundFileRef = ((i + 1) % checkpointCountPerLap == 0 ? lapSoundFileRef : checkpointSoundFileRef) ?? throw new UnreachableException();
            
            //Console.Write("-> Creating checkpoint sound media block ({0})... ", Path.GetFileName(soundFileRef.FilePath));
            soundMediaBlocks[i] = CreateCheckpointSoundMediaBlock(time, soundFileRef);
            //Console.WriteLine("Done");
        }

        //Console.Write("Cleaning up overlapping... ");
        ClearOverlappingOnText(textMediaBlocks, textShadowMediaBlocks, textMultilapMediaBlocks, textDeltaMediaBlocks, textMultilapCrossMediaBlocks);
        ClearOverlappingOnSound(soundMediaBlocks);
        //Console.WriteLine("Done");

        //Console.Write("Creating checkpoint text and shadow media tracks for the media blocks... ");
        var trackList = new List<CGameCtnMediaTrack>
        {
            CreateMediaTrack(textMediaBlocks, name: Config.Dictionary.MediaTrackerTrackCheckpointTime),
            CreateMediaTrack(textShadowMediaBlocks, name: Config.Dictionary.MediaTrackerTrackCheckpointTimeShadow)
        };
        //Console.WriteLine("Done");

        if (deltaGhost is not null)
        {
            //Console.Write("Creating media track for the delta text media blocks... ");
            trackList.Add(CreateMediaTrack(textDeltaMediaBlocks, name: Config.Dictionary.MediaTrackerTrackCheckpointDelta));
            //Console.WriteLine("Done");
        }

        if (Config.IncludeSound)
        {
            //Console.Write("Creating media track for the sound media blocks... ");
            trackList.Add(CreateMediaTrack(soundMediaBlocks, name: Config.Dictionary.MediaTrackerTrackCheckpointSound));
            //Console.WriteLine("Done");
        }

        if (isMultilap)
        {
            //Console.Write("Creating media track for the multilap media blocks... ");
            trackList.Add(CreateMediaTrack(textMultilapMediaBlocks, name: Config.Dictionary.MediaTrackerTrackCheckpointLap));
            trackList.Add(CreateMediaTrack(textMultilapCrossMediaBlocks, name: Config.Dictionary.MediaTrackerTrackCheckpointLapCross));
            //Console.WriteLine("Done");
        }

        //Console.Write("Creating the final media clip from media tracks... ");
        var clip = CreateMediaClip(trackList);
        //Console.WriteLine("Done");

        var mapName = map?.MapName ?? map?.MapInfo?.Id ?? ghost.Validate_ChallengeUid ?? "unknownmap";
        var raceTime = ghost.RaceTime.ToTmString(useApostrophe: true) ?? "unfinished";
        var author = ghost.GhostNickname ?? ghost.GhostLogin ?? "unnamed";

        var pureFileName = $"ClipCheckpoint_{TextFormatter.Deformat(mapName)}_{raceTime}_{TextFormatter.Deformat(author)}.Clip.Gbx";
        var validFileName = string.Join("_", RegexUtils.GetExtendedAsciiValid(pureFileName).Split(Path.GetInvalidFileNameChars()));

        var forManiaPlanet = GameVersion.IsManiaPlanet(ghost);
        var dir = forManiaPlanet ? "Replays/Clips/ClipCheckpoint" : "Tracks/ClipCheckpoint";

        return new(clip, $"{dir}/{validFileName}", forManiaPlanet);
    }

    private CGameCtnMediaBlockText CreateDeltaMediaBlock(TimeSpan time, bool isFromTM2, TimeSpan interval)
    {
        var deltaTimeStr = interval.ToTmString(useHundredths: !isFromTM2);
        var isPositiveDelta = interval > TimeSpan.Zero;

        var deltaTimeTextWithoutFormat = (isPositiveDelta ? "+" : "") + deltaTimeStr;
        var deltaTimeText = string.Format(Config.TextDeltaFormat, deltaTimeTextWithoutFormat);

        var deltaColor = interval.Ticks switch
        {
            > 0 => Config.DeltaPositiveColor,
            < 0 => Config.DeltaNegativeColor,
            _ => Config.DeltaEqualColor
        };

        //Console.Write("-> Creating checkpoint delta text media block ({0})... ", deltaTimeTextWithoutFormat);
        var mediaBlock = CreateCheckpointTextMediaBlock(time,
            deltaTimeText,
            offsetPosition: Config.DeltaTimePositionOffset,
            color: deltaColor,
            scale: Config.DeltaTimeScale);
        //Console.WriteLine("Done");

        return mediaBlock;
    }
    
    private void ClearOverlappingOnText(params CGameCtnMediaBlockText[][] textMediaBlockSets)
    {
        foreach (var mediaBlocks in textMediaBlockSets)
        {
            for (var i = 0; i < mediaBlocks.Length - 1; i++)
            {
                var mediaBlock = mediaBlocks[i];
                var nextMediaBlock = mediaBlocks[i + 1];

                var endingKey = mediaBlock.Effect.Keys.Last();
                var nextStartingKey = nextMediaBlock.Effect.Keys.First();

                if (nextStartingKey.Time >= endingKey.Time)
                    continue;

                var timeDifference = endingKey.Time - nextStartingKey.Time;

                if (timeDifference > Config.AnimationOutLength)
                {
                    mediaBlock.Effect.Keys.RemoveAt(mediaBlock.Effect.Keys.Count - 1);
                    mediaBlock.Effect.Keys.Last().Time = nextStartingKey.Time;
                    continue;
                }

                endingKey.Time = nextStartingKey.Time;
            }
        }
    }

    private static void ClearOverlappingOnSound(CGameCtnMediaBlockSound[] mediaBlocks)
    {
        for (var i = 0; i < mediaBlocks.Length - 1; i++)
        {
            var mediaBlock = mediaBlocks[i];
            var nextMediaBlock = mediaBlocks[i + 1];

            var endingKey = mediaBlock.Keys.Last();
            var nextStartingKey = nextMediaBlock.Keys.First();

            if (nextStartingKey.Time < endingKey.Time)
            {
                endingKey.Time = nextStartingKey.Time;
            }
        }
    }

    private FileRef CreateCheckpointSoundFileRef()
    {
        var soundUrl = map?.Collection.ToString() switch
        {
            "Alpine" => "https://gbx.bigbang1112.cz/assets/tools/clip-checkpoint/sounds/AlpineCheckPoint.ogg",
            "Rally" => "https://gbx.bigbang1112.cz/assets/tools/clip-checkpoint/sounds/RallyCheckPoint.ogg",
            "Speed" => "https://gbx.bigbang1112.cz/assets/tools/clip-checkpoint/sounds/SpeedCheckPoint.ogg",
            _ => "https://gbx.bigbang1112.cz/assets/tools/clip-checkpoint/sounds/RaceCheckPoint.ogg"
        };

        return CreateSoundFileRef(soundUrl);
    }

    private FileRef CreateLapSoundFileRef()
    {
        return CreateSoundFileRef("https://gbx.bigbang1112.cz/assets/tools/clip-checkpoint/sounds/RaceLap.ogg");
    }

    private FileRef CreateSoundFileRef(string soundUrl)
    {
        return new FileRef(
            version: Config.Legacy ? (byte)1 : (byte)2,
            checksum: FileRef.DefaultChecksum,
            filePath: (GameVersion.IsManiaPlanet(ghosts.First()) ? "Media" : "MediaTracker") + @"\Sounds\ClipCheckpoint\" + Path.GetFileName(soundUrl),
            locatorUrl: soundUrl);
    }

    private CGameCtnMediaBlockText CreateLapMediaBlock(TimeSpan time, CGameCtnGhost.Checkpoint[] checkpoints, int currentCheckpointIndex, int checkpointCountPerLap, bool isFromTM2)
    {
        var i = currentCheckpointIndex;

        // Gets an index of the lap time that will be substracted
        var indexOfComparedLap = i - (i % checkpointCountPerLap) - 1;

        var checkpointOnLap = checkpoints[indexOfComparedLap];

        var lapTime = time - checkpointOnLap.Time;
        var lapTimeStr = lapTime.ToTmString(useHundredths: !isFromTM2);
        var lapTimeText = string.Format(Config.TextLapFormat, lapTimeStr);

        return CreateCheckpointTextMediaBlock(time,
            lapTimeText,
            offsetPosition: Config.LapTimePositionOffset,
            color: Config.LapTimeColor,
            scale: Config.LapTimeScale);
    }

    private CGameCtnMediaBlockText CreateLapCrossMediaBlock(TimeInt32 time, int lapNumber, int laps)
    {
        var lapText = string.Format(Config.TextLapCrossFormat, lapNumber, laps);

        return CreateCheckpointTextMediaBlock(time,
            lapText,
            offsetPosition: Config.LapCrossPositionOffset,
            color: Config.LapCrossColor,
            scale: Config.LapCrossScale);
    }

    private CGameCtnMediaBlockSound CreateCheckpointSoundMediaBlock(TimeSpan time, FileRef soundFileRef)
    {
        var soundKeys = GetCheckpointSoundMediaBlockKeys(time);

        var soundBuilder = CGameCtnMediaBlockSound.Create()
            .WithSound(soundFileRef)
            .WithKeys(soundKeys);

        return Config.Legacy
            ? soundBuilder.ForTMSX().Build()
            : soundBuilder.ForTMUF().Build();
    }

    private CGameCtnMediaBlockSound.Key[] GetCheckpointSoundMediaBlockKeys(TimeSpan time)
    {
        var timeGraph = new TimeSpan[]
        {
            TimeSpan.Zero,
            Config.Length
        };

        return Enumerable.Range(0, 2).Select(i => new CGameCtnMediaBlockSound.Key
        {
            Time = time + timeGraph[i],
            Volume = Config.Volume
        }).ToArray();
    }

    private CGameCtnMediaClip CreateMediaClip(List<CGameCtnMediaTrack> tracks)
    {
        var clipBuilder = CGameCtnMediaClip.Create()
            .WithTracks(tracks);

        return Config.Legacy
            ? clipBuilder.ForTMSX().Build()
            : clipBuilder.ForTMUF().Build();
    }

    private CGameCtnMediaTrack CreateMediaTrack(CGameCtnMediaBlock[] blocks, string name)
    {
        var builder = CGameCtnMediaTrack.Create()
            .WithName(name)
            .WithBlocks(blocks);

        return Config.Legacy
            ? builder.ForTMSX().Build()
            : builder.ForTMUF().Build();
    }

    private CGameCtnMediaBlockText CreateCheckpointTextMediaBlock(
        TimeSpan time,
        string text,
        Vec2 offsetPosition = default,
        float offsetDepth = default,
        Vec4 color = default,
        Vec2? scale = null)
    {
        var keys = GetCheckpointTextMediaBlockKeys(time, offsetPosition, offsetDepth, scale, color.W);

        var effectBuilder = CControlEffectSimi.Create()
            .WithKeys(keys)
            .Centered();

        var effect = Config.Legacy
            ? effectBuilder.ForTMSX().Build()
            : effectBuilder.ForTMUF().Interpolated().Build();

        var mediaBlockBuilder = CGameCtnMediaBlockText.Create(effect)
            .WithText(text)
            .WithColor(new(color.X, color.Y, color.Z));

        return Config.Legacy
            ? mediaBlockBuilder.ForTMSX().Build()
            : mediaBlockBuilder.ForTMUF().Build();
    }

    private List<CControlEffectSimi.Key> GetCheckpointTextMediaBlockKeys(TimeSingle time,
        Vec2 offsetPosition = default,
        float offsetDepth = default,
        Vec2? scale = null,
        float opacity = 1)
    {
        // Progression of time across the keys
        var timeGraph = new TimeSpan[]
        {
            TimeSingle.Zero,
            Config.AnimationInLength,
            Config.Length - Config.AnimationOutLength,
            Config.Length
        };

        // Progression of opacity across the keys
        var opacityGraph = new float[]
        {
            0, opacity, opacity, 0
        };

        var staticPosition = (Config.Position + offsetPosition * (Config.AspectRatio.Y / Config.AspectRatio.X, 1));

        var staticScale = Config.Scale * scale.GetValueOrDefault((1, 1));

        var scaleGraph = new Vec2[]
        {
            Config.AnimationInScale,
            (1, 1),
            (1, 1),
            Config.AnimationOutScale
        };

        var depth = Config.Depth;

        return Enumerable.Range(0, 4).Select(i => new CControlEffectSimi.Key
        {
            Time = time + timeGraph[i],
            Opacity = opacityGraph[i],
            Position = staticPosition,
            Scale = staticScale * scaleGraph[i],
            Depth = depth + offsetDepth
        }).ToList();
    }

    private int GetNumberOfLaps(string? raceSettingsXml)
    {
        if (raceSettingsXml is null || raceSettingsXml == "1P-Time")
        {
            return GetNbLapsFromMap();
        }

        var readerSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };

        using var strReader = new StringReader(raceSettingsXml);
        using var reader = XmlReader.Create(strReader, readerSettings);

        try
        {
            reader.ReadToDescendant("laps");

            return reader.ReadElementContentAsInt();
        }
        catch
        {
            return GetNbLapsFromMap();
        }
    }

    private int GetNbLapsFromMap()
    {
        return map?.TMObjective_IsLapRace.GetValueOrDefault() == true
            ? map.TMObjective_NbLaps.GetValueOrDefault()
            : 1;
    }
}
