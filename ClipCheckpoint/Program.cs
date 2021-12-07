using BigBang1112.ClipCheckpoint.Converters;
using BigBang1112.ClipCheckpoint.Exceptions;
using GBX.NET;
using GBX.NET.Engines.Game;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using YamlDotNet.Serialization;

namespace BigBang1112.ClipCheckpoint;

class Program
{
    static readonly string rootPath = Path.GetDirectoryName(typeof(Program).Assembly.Location) + "/";
    static bool deltaFlag = false; // Added a flag for delta comparison

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
                Console.Write("Please drag and drop GBX files onto the executable. Press any key to continue...");
                Console.ReadKey(intercept: true);
                return;
        }
        else if (args.Length > 1) // If there are more than 1 replays we ask if we would like to compare deltas.
        {
            System.Console.WriteLine("Detected 2 or more replays.");
            System.Console.WriteLine("Would you like to compare deltas? (Y/N)");
            switch (Console.ReadLine())
            {
                case "Y":
                case "y":
                    deltaFlag = true;
                    break;
                default:
                    break;
            }
        }

        var suffix = "-CPs";
        var outputFolder = Path.Combine(rootPath, "Output");

        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        if (deltaFlag) // Delta comparison
        {
            suffix = "-Deltas";
            // Ask the user which replay will be compared to
            System.Console.WriteLine("Fetched replays:");
            // Loop and show user's input file names with index values.
            for (int i = 0; i < args.Length - 1; i++)
            {
                System.Console.WriteLine("{0}| {1}", i, args[i]);
            }
            System.Console.WriteLine("Please enter the number of the replay to compare to");
            // Loop till we get a real number that fits (is an int and is in range)
            int deltaIndex = -1; // Stores the index of the delta replay in string[] args
            while (deltaIndex == -1 || deltaIndex > args.Length - 1)
            {
                int.TryParse(Console.ReadLine(), out deltaIndex);
                System.Console.WriteLine("Didn't recieve a valid number! try again.");
                Console.ReadKey(intercept: true);
            }
            // Now that we have a replay to compare to, we can run all files (except the chosen delta) compared to the delta.
            // TODO Call ProcessFile()
            // TODO Edit ClipCheckpointIO.cs to fit delta comparison
        }
        else // No delta comparison
        {
            foreach (var fileName in args)
            {
                ProcessFile(fileName, suffix, outputFolder);
            }
        }
        

        Console.WriteLine();
        Console.Write("Finished! Press any key to continue... ");
        Console.ReadKey(intercept: true);
    }

    static void ProcessFile(string fileName, string suffix, string outputFolder)
    {
        if (!File.Exists(fileName))
        {
            Console.WriteLine("{0} does not exist.", fileName);
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

        var config = GetOrCreateConfig();
        var io = new ClipCheckpointIO(node, config);

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
