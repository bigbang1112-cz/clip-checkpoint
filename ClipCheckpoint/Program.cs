using BigBang1112.ClipCheckpoint;
using BigBang1112.ClipCheckpoint.Converters;
using BigBang1112.ClipCheckpoint.Exceptions;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.MwFoundations;
using System.Globalization;
using YamlDotNet.Serialization;

if (args.Length == 0)
{
    Console.Write("Please drag and drop GBX files onto the executable. Press any key to continue...");
    Console.ReadKey(intercept: true);
    return;
}

var rootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location) + "/";
var suffix = "-CPs";
var outputFolder = Path.Combine(rootPath, "Output");

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var nodes = args.Where(FileExists)
    .Select(NodeFromFileName)
    .Where(x => x is not null)
    .ToArray();

if (nodes.Length == 0)
{
    Console.WriteLine();
    Console.Write("No valid files found. Press any key to continue... ");
    Console.ReadKey(intercept: true);
    return;
}

Console.WriteLine();

var deltaFlag = false;

if (args.Length > 1) // If there is more than 1 replay we ask if we would like to compare deltas.
{
    Console.WriteLine("Detected 2 or more files.");
    Console.WriteLine();
    Console.WriteLine("Would you like to compare deltas? (Y/N)");

    deltaFlag = Console.ReadLine()?.ToLower() == "y";
}

Console.WriteLine();

if (deltaFlag) // Delta comparison
{
    ProcessDeltaMode();
}
else // No delta comparison
{
    ProcessNormalMode();
}

Console.WriteLine();
Console.Write("Finished! Press any key to continue... ");
Console.ReadKey(intercept: true);

#region Methods

bool FileExists(string fileName)
{
    if (File.Exists(fileName))
    {
        Console.Write("File \"{0}\" found... ", Path.GetFileName(fileName));
        return true;
    }

    Console.WriteLine("File \"{0}\" not found... ", Path.GetFileName(fileName));
    return false;
}

CMwNod NodeFromFileName(string fileName)
{
    try
    {
        var node = GameBox.ParseNode(fileName);

        if (node is null)
        {
            Console.WriteLine("Fail.");
        }
        else
        {
            Console.WriteLine("Success.");
        }

        return node!;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);

        return null!;
    }
}

void ProcessNormalMode()
{
    foreach (var node in nodes)
    {
        Process(node);
    }
}

void ProcessDeltaMode()
{
    // Ask the user which replay will be compared to
    Console.WriteLine("Fetched replays:");

    // Loop and show user's input file names with index values.
    for (var i = 0; i < nodes.Length; i++)
    {
        Console.WriteLine("[{0}] {1}", i, Path.GetFileName(nodes[i]!.GBX!.FileName));
    }

    Console.WriteLine();

    Console.Write("Please enter the number of the focused replay: ");

    var mainIndex = GetIndex();

    Console.Write("Please enter the number of the replay used to substract checkpoint times: ");

    // Index of the delta replay in string[] args
    int deltaIndex;

    do
    {
        deltaIndex = GetIndex();

        if (deltaIndex == mainIndex)
            Console.Write("Cannot be the same as the focused replay... ");
    }
    while (deltaIndex == mainIndex);

    Console.WriteLine();

    // Now that we have a replay to compare to, we can run all files (except the chosen delta) compared to the delta.
    Process(mainNode: nodes[mainIndex], deltaNode: nodes[deltaIndex]);
}

int GetIndex()
{
    // Index of the delta replay in string[] args
    int index;

    // Loop till we get a real number that fits (is an int and is in range)
    while (!int.TryParse(Console.ReadLine(), out index) || !IsInRange(nodes, index))
    {
        Console.Write("Didn't recieve a valid number! Try again. ");
    }

    return index;
}

bool IsInRange<T>(T[] array, int index)
{
    return index >= 0 && index < array.Length;
}

void Process(CMwNod mainNode, CMwNod? deltaNode = null)
{
    var config = GetOrCreateConfig();
    var io = new ClipCheckpointIO(mainNode, config, deltaNode);

    CGameCtnMediaClip result;

    try
    {
        result = io.Execute();
    }
    catch (NoGhostException ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(ex.Message);
        Console.ResetColor();
        return;
    }
    catch (NoCheckpointsException ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(ex.Message);
        Console.ResetColor();
        return;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex);
        Console.ResetColor();
        return;
    }

    var newFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(mainNode!.GBX!.FileName)) + suffix + ".Clip.Gbx";

    if (!string.IsNullOrWhiteSpace(outputFolder))
        newFileName = Path.Combine(outputFolder, newFileName);

    Directory.CreateDirectory(outputFolder);

    result.Save(newFileName, remap: config.Legacy ? IDRemap.TrackMania2006 : IDRemap.Latest);
}

ClipCheckpointConfig GetOrCreateConfig()
{
    var configYml = Path.Combine(rootPath, "Config.yml");

    if (File.Exists(configYml))
    {
        using var r = File.OpenText(configYml);
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new TimeSpanConverter())
            .WithTypeConverter(new Vec2Converter())
            .WithTypeConverter(new Vec3Converter())
            .Build();

        try
        {
            var config = deserializer.Deserialize<ClipCheckpointConfig>(r);

            if (config is null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nConfig is empty. Continuing with default configuration...");
                Console.ResetColor();

                return new ClipCheckpointConfig();
            }

            return config;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(ex);
            Console.WriteLine("\nContinuing with default configuration...");
            Console.ResetColor();

            return new ClipCheckpointConfig();
        }
    }

    var defaultConfig = new ClipCheckpointConfig();

    using var w = File.CreateText(configYml);
    var serializer = new SerializerBuilder()
        .WithTypeConverter(new TimeSpanConverter())
        .WithTypeConverter(new Vec2Converter())
        .WithTypeConverter(new Vec3Converter())
        .Build();
    serializer.Serialize(w, defaultConfig);

    return defaultConfig;
}

#endregion