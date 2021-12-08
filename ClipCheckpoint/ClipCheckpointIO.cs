using BigBang1112.ClipCheckpoint.Exceptions;
using BigBang1112.ClipCheckpoint.Extensions;
using GBX.NET;
using GBX.NET.Engines.Control;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.MwFoundations;
using System.Xml;
using TmEssentials;

namespace BigBang1112.ClipCheckpoint;

public class ClipCheckpointIO
{
    public CGameCtnGhost Input { get; }
    public CGameCtnGhost? DeltaInput { get; }
    public CGameCtnChallenge? Map { get; }
    public ClipCheckpointConfig Config { get; }

    public ClipCheckpointIO(CMwNod node, ClipCheckpointConfig? config = null, CMwNod? deltaNode = null)
    {
        Input = node switch
        {
            CGameCtnReplayRecord replay => GetGhostFromReplay(replay),
            CGameCtnGhost ghost => ghost,
            CGameCtnMediaClip clip => GetFirstGhostFromClip(clip),
            _ => throw new NoGhostException(),
        };

        switch (node)
        {
            case CGameCtnReplayRecord replay:
                Map = replay.Challenge;
                break;
        }

        Config = config ?? new ClipCheckpointConfig();

        if (deltaNode is not null)
        {
            DeltaInput = deltaNode switch
            {
                CGameCtnReplayRecord deltaReplay => GetGhostFromReplay(deltaReplay),
                CGameCtnGhost deltaGhost => deltaGhost,
                CGameCtnMediaClip deltaClip => GetFirstGhostFromClip(deltaClip),
                _ => null,
            };

            if (DeltaInput is null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Input used to calculate delta has no ghost inside. Clip will be generated without comparison.");
                Console.ResetColor();
            }
        }
    }

    public CGameCtnMediaClip Execute()
    {
        Console.Write("Extracting number of laps... ");
        var laps = GetNumberOfLaps(Input.Validate_RaceSettings);

        var isMultilap = laps > 1;
        Console.WriteLine(isMultilap ? "Multiple laps ({0})" : "No laps", laps);

        var isFromTM2 = IsFromTM2();
        Console.WriteLine(isFromTM2 ? "Ghost is from TM2 or higher version." : "Ghost is from TMUF or lower version.");

        var checkpointSoundFileRef = default(FileRef);
        var lapSoundFileRef = default(FileRef);

        if (Config.IncludeSound)
        {
            Console.Write("Creating the checkpoint sound file reference... ");
            checkpointSoundFileRef = CreateCheckpointSoundFileRef();
            Console.WriteLine("Done");

            Console.Write("Creating the lap sound file reference... ");
            lapSoundFileRef = CreateLapSoundFileRef();
            Console.WriteLine("Done");
        }

        var checkpoints = Input.Checkpoints;

        if (checkpoints is null || checkpoints.Length == 0)
            throw new NoCheckpointsException();

        var otherGhostCpTimes = DeltaInput?.Checkpoints?
            .Select(x => x.Time ?? throw new CheckpointIsMinusOneException())
            .ToArray();

        Console.WriteLine("Checkpoints extracted.");

        // 'Real' CP count depending if also shown on finish line or not
        var checkpointCount = Config.IncludeFinish ? checkpoints.Length : checkpoints.Length - 1;
        Console.WriteLine("Checkpoint count: " + checkpointCount);

        var checkpointCountPerLap = checkpoints.Length / laps;
        Console.WriteLine("Checkpoint count per lap: " + checkpointCountPerLap);

        var textMediaBlocks = new CGameCtnMediaBlockText[checkpointCount];
        var textShadowMediaBlocks = new CGameCtnMediaBlockText[checkpointCount]; // TODO: layering
        var textMultilapMediaBlocks = new CGameCtnMediaBlockText[checkpoints.Length - checkpointCountPerLap];
        var textDeltaMediaBlocks = new CGameCtnMediaBlockText[checkpoints.Length];
        var soundMediaBlocks = new CGameCtnMediaBlockSound[Config.IncludeSound ? checkpointCount : 0];

        for (var i = 0; i < checkpoints.Length; i++)
        {
            Console.WriteLine("Checkpoint {0}:", i + 1);

            var cp = checkpoints[i];

            if (!cp.Time.TryValue(out TimeSpan time))
            {
                if (DeltaInput is null)
                {
                    Console.WriteLine("-> -1 time??? Skipping.");
                    continue;
                }

                throw new CheckpointIsMinusOneException();
            }

            // Lap MT blocks - if lap race - and an unique lap time was reached
            if (isMultilap && i >= checkpointCountPerLap)
            {
                // Index to use for assinging to the lap MT block array
                var index = i - checkpointCountPerLap;

                Console.Write("-> Creating lap text media block... ");
                textMultilapMediaBlocks[index] = CreateLapMediaBlock(
                    time,
                    checkpoints,
                    currentCheckpointIndex: i,
                    checkpointCountPerLap,
                    isFromTM2);
                Console.WriteLine("Done");
            }

            if (DeltaInput is not null)
            {
                var otherGhostTime = otherGhostCpTimes![i];

                // Calculate interval 
                var interval = cp.Time.GetValueOrDefault() - otherGhostTime;

                textDeltaMediaBlocks[i] = CreateDeltaMediaBlock(time, isFromTM2, interval);
            }

            // If the REAL checkpoint count was reached, ignore the rest of the checkpoints array
            if (i == checkpointCount)
            {
                Console.WriteLine("Skipping the finish checkpoint time.");
                break;
            }

            // Actual checkpoint MT blocks

            var timeStr = cp.Time.ToTmString(useHundredths: !isFromTM2);
            var timeText = string.Format(Config.TextCheckpointFormat, timeStr);

            if (Map?.Mode == CGameCtnChallenge.PlayMode.Stunts)
            {
                timeText += " " + string.Format(Config.TextStuntsFormat, cp.StuntsScore);
            }

            Console.Write("-> Creating checkpoint text media block ({0})... ", timeStr);
            textMediaBlocks[i] = CreateCheckpointTextMediaBlock(time,
                timeText,
                color: Config.Color);
            Console.WriteLine("Done");

            Console.Write("-> Creating checkpoint text shadow media block... ");
            textShadowMediaBlocks[i] = CreateCheckpointTextMediaBlock(time,
                timeText,
                offsetPosition: -Config.ShadowHeight,
                offsetDepth: Config.ShadowDepthOffset,
                color: Config.ShadowColor);
            Console.WriteLine("Done");

            if (!Config.IncludeSound)
                continue;

            var soundFileRef = (i + 1) % checkpointCountPerLap == 0 ? lapSoundFileRef : checkpointSoundFileRef;

            if (soundFileRef is null)
                throw new ThisShouldNotHappenException();

            Console.Write("-> Creating checkpoint sound media block ({0})... ", Path.GetFileName(soundFileRef.FilePath));
            soundMediaBlocks[i] = CreateCheckpointSoundMediaBlock(time, soundFileRef);
            Console.WriteLine("Done");
        }

        Console.Write("Cleaning up overlapping... ");
        ClearOverlappingOnText(textMediaBlocks, textShadowMediaBlocks, textMultilapMediaBlocks, textDeltaMediaBlocks);
        ClearOverlappingOnSound(soundMediaBlocks);
        Console.WriteLine("Done");

        Console.Write("Creating checkpoint text and shadow media tracks for the media blocks... ");
        var trackList = new List<CGameCtnMediaTrack>
        {
            CreateMediaTrack(textMediaBlocks, name: Config.TrackNameCheckpointTime),
            CreateMediaTrack(textShadowMediaBlocks, name: Config.TrackNameCheckpointTimeShadow)
        };
        Console.WriteLine("Done");

        if (DeltaInput is not null)
        {
            Console.Write("Creating media track for the delta text media blocks... ");
            trackList.Add(CreateMediaTrack(textDeltaMediaBlocks, name: Config.TrackNameCheckpointDelta));
            Console.WriteLine("Done");
        }

        if (Config.IncludeSound)
        {
            Console.Write("Creating media track for the sound media blocks... ");
            trackList.Add(CreateMediaTrack(soundMediaBlocks, name: Config.TrackNameCheckpointSound));
            Console.WriteLine("Done");
        }

        if (isMultilap)
        {
            Console.Write("Creating media track for the multilap media blocks... ");
            trackList.Add(CreateMediaTrack(textMultilapMediaBlocks, name: Config.TrackNameCheckpointLap));
            Console.WriteLine("Done");
        }

        Console.Write("Creating the final media clip from media tracks... ");
        var clip = CreateMediaClip(trackList);
        Console.WriteLine("Done");

        return clip;
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

        Console.Write("-> Creating checkpoint delta text media block ({0})... ", deltaTimeTextWithoutFormat);
        var mediaBlock = CreateCheckpointTextMediaBlock(time,
            deltaTimeText,
            offsetPosition: Config.DeltaTimePositionOffset,
            color: deltaColor,
            scale: Config.DeltaTimeScale);
        Console.WriteLine("Done");

        return mediaBlock;
    }

    private static CGameCtnGhost GetGhostFromReplay(CGameCtnReplayRecord replay)
    {
        if (replay.Ghosts.Length > 0)
            return replay.Ghosts[0];

        if (replay.Clip is null)
            throw new NoGhostException();

        return GetFirstGhostFromClip(replay.Clip);
    }

    private static CGameCtnGhost GetFirstGhostFromClip(CGameCtnMediaClip clip)
    {
        foreach (var track in clip.Tracks)
            foreach (var block in track.Blocks)
                if (block is CGameCtnMediaBlockGhost blockGhost)
                    return blockGhost.GhostModel;

        throw new NoGhostException();
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
        var soundUrl = Map?.Collection.ToString() switch
        {
            "Alpine" => Config.AlpineCheckpointSoundUrl,
            "Rally" => Config.RallyCheckpointSoundUrl,
            "Speed" => Config.SpeedCheckpointSoundUrl,
            _ => Config.CheckpointSoundUrl
        };

        return CreateSoundFileRef(soundUrl);
    }

    private FileRef CreateLapSoundFileRef()
    {
        return CreateSoundFileRef(Config.LapSoundUrl);
    }

    private FileRef CreateSoundFileRef(string soundUrl)
    {
        return new FileRef(
            version: Config.Legacy ? (byte)1 : (byte)2,
            checksum: FileRef.DefaultChecksum,
            filePath: @"MediaTracker\Sounds\" + Path.GetFileName(soundUrl),
            locatorUrl: new Uri(soundUrl));
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
        Vec3 color = default,
        Vec2? scale = null)
    {
        var keys = GetCheckpointTextMediaBlockKeys(time, offsetPosition, offsetDepth, scale);

        var effectBuilder = CControlEffectSimi.Create()
            .WithKeys(keys)
            .Centered();

        var effect = Config.Legacy
            ? effectBuilder.ForTMSX().Build()
            : effectBuilder.ForTMUF().Interpolated().Build();

        var mediaBlockBuilder = CGameCtnMediaBlockText.Create()
            .WithText(text)
            .WithEffect(effect)
            .WithColor(color);

        return Config.Legacy
            ? mediaBlockBuilder.ForTMSX().Build()
            : mediaBlockBuilder.ForTMUF().Build();
    }

    private List<CControlEffectSimi.Key> GetCheckpointTextMediaBlockKeys(
        TimeSpan time,
        Vec2 offsetPosition = default,
        float offsetDepth = default,
        Vec2? scale = null)
    {
        // Progression of time across the keys
        var timeGraph = new TimeSpan[]
        {
            TimeSpan.Zero,
            Config.AnimationInLength,
            Config.Length - Config.AnimationOutLength,
            Config.Length
        };

        // Progression of opacity across the keys
        var opacityGraph = new int[]
        {
            0, 1, 1, 0
        };

        var staticPosition = (Config.Position + offsetPosition) * GetAspectRatioMultiplier();

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

        int GetNbLapsFromMap()
        {
            return Map?.TMObjective_IsLapRace.GetValueOrDefault() == true
                ? Map.TMObjective_NbLaps.GetValueOrDefault()
                : 1;
        }
    }

    private bool IsFromTM2()
    {
        return Input.GetChunk<CGameCtnGhost.Chunk0309201C>() is not null;
    }

    private Vec2 GetAspectRatioMultiplier() => (Config.AspectRatio.Y / Config.AspectRatio.X, 1);
}
