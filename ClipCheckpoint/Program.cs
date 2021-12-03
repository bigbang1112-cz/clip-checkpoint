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

    static void Main(string[] args)
    {
        if (args.Length == 0) return;

        var suffix = "-CPs";
        var outputFolder = Path.Combine(rootPath, "Output");

        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        foreach (var fileName in args)
        {
            ProcessFile(fileName, suffix, outputFolder);
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
