using BigBang1112.ClipCheckpoint.Converters;
using BigBang1112.ClipCheckpoint.Exceptions;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.MwFoundations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using YamlDotNet.Serialization;

namespace BigBang1112.ClipCheckpoint;

class Program
{
    static readonly string rootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location) + "/";
    static readonly string suffix = "-CPs";
    static readonly string outputFolder = Path.Combine(rootPath, "Output");

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Write("Please drag and drop GBX files onto the executable. Press any key to continue...");
            Console.ReadKey(intercept: true);
            return;
        }

        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var deltaFlag = false;
        
        if (args.Length > 1) // If there is more than 1 replay we ask if we would like to compare deltas.
        {
            Console.WriteLine("Detected 2 or more replays.");
            Console.WriteLine("Would you like to compare deltas? (Y/N)");

            deltaFlag = Console.ReadLine()?.ToLower() == "y";
        }

        if (deltaFlag) // Delta comparison
        {
            ProcessDeltaMode(args);
        }
        else // No delta comparison
        {
            ProcessNormalMode(args);
        }

        Console.WriteLine();
        Console.Write("Finished! Press any key to continue... ");
        Console.ReadKey(intercept: true);
    }

    private static void ProcessNormalMode(string[] args)
    {
        foreach (var fileName in args)
        {
            ProcessFile(fileName);
        }
    }

    private static void ProcessDeltaMode(string[] args)
    {
        // Ask the user which replay will be compared to
        Console.WriteLine("Fetched replays:");

        // Loop and show user's input file names with index values.
        for (var i = 0; i < args.Length; i++)
        {
            Console.WriteLine("{0}| {1}", i, args[i]);
        }

        Console.WriteLine();

        Console.WriteLine("Please enter the number of the focused replay:");
        
        var mainIndex = GetIndex(args);

        Console.WriteLine("Please enter the number of the replay used to substract checkpoint times:");

        // Index of the delta replay in string[] args
        var deltaIndex = GetIndex(args);

        // Now that we have a replay to compare to, we can run all files (except the chosen delta) compared to the delta.
        ProcessFile(fileName: args[mainIndex], deltaFileName: args[deltaIndex]);
    }

    private static int GetIndex(string[] args)
    {
        // Index of the delta replay in string[] args
        int index;

        // Loop till we get a real number that fits (is an int and is in range)
        while (!int.TryParse(Console.ReadLine(), out index) || !IsInRange(args, index))
        {
            Console.WriteLine("Didn't recieve a valid number! try again.");
        }

        return index;
    }

    private static bool IsInRange(string[] array, int index)
    {
        return index >= 0 && index < array.Length;
    }

    static void ProcessFile(string fileName, string? deltaFileName = null)
    {
        if (!File.Exists(fileName))
        {
            Console.WriteLine("{0} does not exist.", fileName);
            return;
        }

        var deltaExists = File.Exists(deltaFileName);

        if (!deltaExists)
        {
            Console.WriteLine("{0} does not exist.", deltaFileName);
            return;
        }

        Console.Write("Reading the GBX file... ");

        var node = GameBox.ParseNode(fileName);

        if (node is null)
        {
            Console.WriteLine("GBX is not readable by the program.");
            return;
        }

        Console.WriteLine("Done");

        var deltaNode = default(CMwNod?);

        if (deltaExists)
        {
            Console.WriteLine("Reading the chosen delta GBX file...");

            deltaNode = GameBox.ParseNode(deltaFileName!);

            if (deltaNode is null)
            {
                Console.WriteLine("GBX is not readable by the program.");
                return;
            }
        }

        var config = GetOrCreateConfig();
        var io = new ClipCheckpointIO(node, config, deltaNode: deltaNode);

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

        var newFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileName)) + suffix + ".Clip.Gbx";

        if (!string.IsNullOrWhiteSpace(outputFolder))
            newFileName = Path.Combine(outputFolder, newFileName);

        Directory.CreateDirectory(outputFolder);

        result.Save(newFileName, remap: config.Legacy ? IDRemap.TrackMania2006 : IDRemap.Latest);
    }

    private static ClipCheckpointConfig GetOrCreateConfig()
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
}
