using GBX.NET;
using GbxToolAPI.CLI.GameInstallations;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;

namespace GbxToolAPI.CLI;

public class ToolConsole<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] T> where T : class, ITool
{
    private static readonly string rootPath;

    public static async Task<ToolConsole<T>?> RunAsync(string[] args)
    {
        var c = default(ToolConsole<T>);

        try
        {
            c = await RunCanThrowAsync(args);
        }
        catch (ConsoleFailException ex)
        {
            ConsoleWriteLineWithColor(ex.Message, ConsoleColor.Red);
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            ConsoleWriteLineWithColor(ex.ToString(), ConsoleColor.Red);
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        return c;
    }

    static ToolConsole()
    {
        Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));

        rootPath = Environment.ProcessPath is null ? "" : Path.GetDirectoryName(Environment.ProcessPath) ?? "";
    }

    private static async Task<ToolConsole<T>> RunCanThrowAsync(string[] args)
    {
        Console.WriteLine("Initializing the console...");

        AssetsManager<T>.RunsViaConsole = true;

        var type = typeof(T);

        var outputInterfaces = type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHasOutput<>))
            .ToList();

        if (!outputInterfaces.Any())
        {
            throw new ConsoleFailException("Tool must implement at least one IHasOutput<T> interface.");
        }

        var console = new ToolConsole<T>();

        Console.WriteLine("Using the tool: " + type.GetCustomAttribute<ToolNameAttribute>()?.Name);

        var configProps = GetConfigProps(out var configPropTypes);

        Console.WriteLine("Scanning all the command line arguments...");

        var inputFiles = args.TakeWhile(arg => !arg.StartsWith('-')).ToArray();

        if (inputFiles.Length > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Detected input files:\n- " + string.Join("\n- ", inputFiles.Select(Path.GetFileName)));
        }

        var remainingArgs = args.Skip(inputFiles.Length);

        var consoleOptions = GetConsoleOptions(remainingArgs, configProps, out var configPropsToSet);

        if (!await CheckForUpdatesAsync(type, consoleOptions.NoPause)) // if not continue
        {
            return console;
        }

        var configInstances = GetConfigInstances(configPropTypes, consoleOptions);

        Console.WriteLine();

        var inputByType = CreateInputDictionaryFromFiles(inputFiles);
        var userDataPathDict = new Dictionary<string, string>();

        foreach (var (toolInstance, ctorParams) in ToolConstructorPicker.CreateInstances<T>(inputByType, consoleOptions.SingleOutput))
        {
            var installPath = GetSuitableInstallationPath(ctorParams, consoleOptions);
            var userDataPath = default(string);

            if (installPath is not null)
            {
                if (!userDataPathDict.TryGetValue(installPath, out userDataPath))
                {
                    userDataPath = NadeoIni.Parse(Path.Combine(installPath, "Nadeo.ini")).UserDataDir;
                    userDataPathDict.Add(installPath, userDataPath);
                }
            }

            var finalPath = consoleOptions.OutputDir ?? userDataPath ?? Path.Combine(rootPath, "Output");

            await RunToolInstanceAsync(toolInstance, configInstances, configPropsToSet, finalPath);

            Console.WriteLine();
        }

        Console.WriteLine("Complete!");

        if (!consoleOptions.NoPause)
        {
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        return console;
    }

    private static async ValueTask<bool> CheckForUpdatesAsync(Type type, bool noPause)
    {
        if (type.GetCustomAttribute<ToolGitHubAttribute>() is not ToolGitHubAttribute githubAtt || githubAtt.NoExe)
        {
            return true;
        }

        if (CheckForUpdatesShouldBeSkipped())
        {
            return true;
        }
        
        Console.WriteLine("Checking for updates...");
        
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd($"GbxToolAPI.CLI ({githubAtt.Repository})");

        using var response = await http.GetAsync($"https://api.github.com/repos/{githubAtt.Repository}/releases/latest");

        if (!response.IsSuccessStatusCode || await response.Content.ReadFromJsonAsync<GitHubRelease>() is not GitHubRelease release)
        {
            ConsoleWriteLineWithColor("Could not retrieve releases.", ConsoleColor.Red);
            return true;
        }

        var latestVersion = new Version(release.TagName.TrimStart('v'));
        var installedVersion = type.Assembly.GetName().Version;

        if (installedVersion >= latestVersion)
        {
            Console.WriteLine("No new update detected.");
            return true;
        }

        Console.WriteLine();
        ConsoleWriteLineWithColor($"New update {release.TagName} is now available!\nPlease update to take advantage of new features, bug fixes, and security updates.", ConsoleColor.Green);

        if (noPause)
        {
            Console.WriteLine();
            return true;
        }

        ConsoleWriteWithColor("Press U to open the release page", ConsoleColor.Yellow);
        Console.Write(", or any other key to continue using current version...");
        var pressedKey = Console.ReadKey();

        if (pressedKey.Key == ConsoleKey.U)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"https://github.com/{githubAtt.Repository}/releases/tag/{release.TagName}",
                UseShellExecute = true
            });

            return false;
        }

        Console.WriteLine();

        return true;
    }

    private static bool CheckForUpdatesShouldBeSkipped()
    {
        var lastCheckFilePath = Path.Combine(rootPath, "LastCheckForUpdates.txt");

        if (File.Exists(lastCheckFilePath) && DateTimeOffset.TryParse(File.ReadAllText(lastCheckFilePath), CultureInfo.InvariantCulture, out var lastCheckDateTime))
        {
            if (DateTimeOffset.UtcNow - lastCheckDateTime > TimeSpan.FromHours(1))
            {
                File.WriteAllText(lastCheckFilePath, DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                return true;
            }
        }
        else
        {
            File.WriteAllText(lastCheckFilePath, DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture));
        }

        return false;
    }

    private static void ConsoleWriteLineWithColor(string text, ConsoleColor color)
    {
        var curColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = curColor;
    }

    private static void ConsoleWriteWithColor(string text, ConsoleColor color)
    {
        var curColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ForegroundColor = curColor;
    }

    private static Dictionary<Type, ICollection<object>> CreateInputDictionaryFromFiles(string[] files)
    {
        var dict = new Dictionary<Type, ICollection<object>>();

        foreach (var typeGroup in GetFileObjectInstances(files).GroupBy(obj => obj.GetType()))
        {
            var list = new List<object>();

            foreach (var obj in typeGroup)
            {
                list.Add(obj);
            }

            dict.Add(typeGroup.Key, list);
        }

        return dict;
    }

    private static IEnumerable<object> GetFileObjectInstances(string[] files)
    {
        foreach (var file in files)
        {
            if (IsTextFile(file))
            {
                yield return new TextFile(File.ReadAllText(file));
                continue;
            }

            Console.WriteLine("Parsing " + Path.GetFileName(file) + "...");

            Node? node;

            try
            {
                node = GameBox.ParseNode(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                continue;
            }

            yield return node is null ? new BinFile(File.ReadAllBytes(file)) : node;
        }

        Console.WriteLine();
    }

    private static bool IsTextFile(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var r = new StreamReader(fs, Encoding.UTF8, true, 1024, true);

            while (!r.EndOfStream)
            {
                int charValue = r.Read();
                if (charValue == 0)
                {
                    // file has null byte, considered binary
                    return false;
                }
            }

            // file doesn't contain null bytes or invalid UTF-8 sequences, considered text
            return true;
        }
        catch (DecoderFallbackException)
        {
            // invalid UTF-8 sequence, considered binary
            return false;
        }
    }

    private static string? GetSuitableInstallationPath(object[] ctorParams, ConsoleOptions options)
    {
        foreach (var input in ctorParams)
        {
            var inputOrFirstInput = input;

            if (input is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();

                if (!enumerator.MoveNext())
                {
                    return null;
                }

                inputOrFirstInput = enumerator.Current;
            }

            if (inputOrFirstInput is not Node node)
            {
                continue;
            }

            var isTM2020 = GameVersion.IsTM2020(node);

            if (isTM2020.HasValue && isTM2020.Value)
            {
                return options.Trackmania2020InstallationPath;
            }

            var canBeTurbo = GameVersion.CanBeTMTurbo(node);

            if (canBeTurbo.HasValue && canBeTurbo.Value)
            {
                return options.TrackmaniaTurboInstallationPath;
            }

            var isManiaPlanet = GameVersion.IsManiaPlanet(node);

            if (isManiaPlanet.HasValue && isManiaPlanet.Value)
            {
                return options.ManiaPlanetInstallationPath;
            }

            var isTMF = GameVersion.IsTMF(node);

            if (isTMF.HasValue && isTMF.Value)
            {
                return options.TrackmaniaForeverInstallationPath;
            }
        }

        return null;
    }

    private static async Task RunToolInstanceAsync(T toolInstance, Dictionary<PropertyInfo, Config> configInstances, Dictionary<PropertyInfo, object?> configPropsToSet, string outputDir)
    {
        Console.WriteLine("Running tool instance...");

        foreach (var (configProp, config) in configInstances)
        {
            configProp.SetValue(toolInstance, config);

            foreach (var (prop, value) in configPropsToSet)
            {
                try
                {
                    prop.SetValue(config, value);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException ?? ex;
                }
            }
        }

        if (toolInstance is IHasAssets toolWithAssets)
        {
            await toolWithAssets.LoadAssetsAsync();
        }

        foreach (var produceMethod in typeof(T).GetMethods().Where(m => m.Name == nameof(IHasOutput<object>.Produce)))
        {
            var typeName = GetTypeName(produceMethod.ReturnType);

            Console.WriteLine($"Producing {typeName}...");

            var watch = Stopwatch.StartNew();

            var output = produceMethod.Invoke(toolInstance, null);

            var name = output is null ? "[null]" : GetTypeName(output.GetType());

            Console.WriteLine($"Produced {name}. ({watch.Elapsed.TotalMilliseconds}ms)");

            if (output is null)
            {
                continue;
            }

            var outputSaver = new OutputSaver(output, outputDir);
            outputSaver.Save();
        }
    }

    private static Dictionary<PropertyInfo, Config> GetConfigInstances(IList<PropertyInfo> configPropTypes, ConsoleOptions consoleOptions)
    {
        var configInstances = new Dictionary<PropertyInfo, Config>();

        if (configPropTypes.Count <= 0)
        {
            return configInstances;
        }

        Directory.CreateDirectory(Path.Combine(rootPath, "Config"));

        var customConfig = consoleOptions.CustomConfig ?? "Default";

        foreach (var configProp in configPropTypes)
        {
            var configType = configProp.PropertyType;

            var fileName = Path.Combine(rootPath, "Config", $"{customConfig}{(configPropTypes.Count > 1 ? $"_{configType.Name}" : "")}.yml");

            Config? config;

            if (File.Exists(fileName))
            {
                using var r = new StreamReader(fileName);
                config = (Config)Yml.Deserializer.Deserialize(r, configType)!;
            }
            else
            {
                config = (Config)Activator.CreateInstance(configType)!;
            }

            File.WriteAllText(fileName, Yml.Serializer.Serialize(config));

            configInstances.Add(configProp, config);
        }

        return configInstances;
    }

    private static ConsoleOptions GetConsoleOptions(IEnumerable<string> args, Dictionary<string, PropertyInfo> configProps, out Dictionary<PropertyInfo, object?> configPropsToSet)
    {
        Console.WriteLine();

        var consoleOptionsPath = Path.Combine(rootPath, "ConsoleOptions.yml");

        var updatedAssets = false;

        ConsoleOptions options;

        if (File.Exists(consoleOptionsPath))
        {
            Console.WriteLine("Using existing ConsoleOptions.yml...");

            using var r = new StreamReader(consoleOptionsPath);
            options = Yml.Deserializer.Deserialize<ConsoleOptions>(r)!;
        }
        else
        {
            Console.WriteLine("Creating new ConsoleOptions.yml...");
            options = new ConsoleOptions();

            var games = new List<GameInstallation>
            {
                new TrackmaniaForeverGameInstallation(),
                new ManiaPlanetGameInstallation(),
                new TrackmaniaTurboGameInstallation(),
                new Trackmania2020GameInstallation()
            };

            foreach (var game in games)
            {
                var path = game.SuggestedInstallationPaths.FirstOrDefault(Directory.Exists);

                Console.WriteLine();

                while (true)
                {
                    if (path is not null)
                    {
                        Console.WriteLine($"Found {game.Name} installation at '{path}'.");
                        Console.Write("Agree [y/n]? ");

                        var k = Console.ReadKey();

                        Console.WriteLine();

                        if (k.Key == ConsoleKey.N)
                        {
                            path = null;
                        }
                    }

                    if (path is null)
                    {
                        Console.WriteLine($"Attempted to search for the following installation paths of {game.Name}:");

                        foreach (var p in game.SuggestedInstallationPaths)
                        {
                            Console.WriteLine($"- {p}");
                        }

                        Console.WriteLine($"But it didn't find any existing one.");

                        Console.Write($"Enter your {game.Name} installation path manually (leave empty if not installed or interested): ");

                        path = Console.ReadLine();
                    }

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        break;
                    }

                    if (!Directory.Exists(path))
                    {
                        Console.WriteLine("Directory does not exist.");
                        continue;
                    }

                    if (!File.Exists(Path.Combine(path, game.ExeName + ".exe")))
                    {
                        Console.WriteLine("Correct game executable not found in this directory.");
                        continue;
                    }

                    CopyAssets(path ?? throw new UnreachableException("Path is null but it shouldn't be"), game is not TrackmaniaForeverGameInstallation);

                    game.SetPathFromOptions(options, path);

                    break;
                }
            }

            updatedAssets = true;
        }

        File.WriteAllText(consoleOptionsPath, Yml.Serializer.Serialize(options));

        configPropsToSet = new Dictionary<PropertyInfo, object?>();

        Console.WriteLine();
        Console.Write("Additional arguments: ");

        var argEnumerator = args.GetEnumerator();

        var anyAdditionalArguments = false;

        while (argEnumerator.MoveNext())
        {
            anyAdditionalArguments = true;
            Console.WriteLine();

            var arg = argEnumerator.Current;
            var argLower = arg.ToLowerInvariant();

            switch (argLower)
            {
                case "-nopause":
                    options.NoPause = true;
                    Console.WriteLine($": {arg}");
                    break;
                case "-singleoutput": // Merge will produce only one instance of Tool
                    options.SingleOutput = true;
                    Console.WriteLine($": {arg}");
                    continue;
                case "-config":
                    if (!argEnumerator.MoveNext())
                    {
                        throw new ConsoleFailException("Missing string value for option " + arg);
                    }

                    options.CustomConfig = argEnumerator.Current;
                    Console.WriteLine($": {arg} \"{options.CustomConfig}\"");
                    continue;
                case "-o":
                case "-output":
                    if (!argEnumerator.MoveNext())
                    {
                        throw new ConsoleFailException("Missing string value for option " + arg);
                    }

                    var outputDir = argEnumerator.Current;

                    if (!Directory.Exists(outputDir))
                    {
                        throw new ConsoleFailException("Output directory does not exist: " + outputDir);
                    }

                    options.OutputDir = outputDir;
                    Console.WriteLine($": {arg} \"{outputDir}\"");
                    continue;
                case "-updateassets":
                    var gamesForAssets = new List<GameInstallation>
                    {
                        new TrackmaniaForeverGameInstallation(),
                        new ManiaPlanetGameInstallation(),
                        new TrackmaniaTurboGameInstallation(),
                        new Trackmania2020GameInstallation()
                    };

                    foreach (var game in gamesForAssets)
                    {
                        var path = game.GetPathFromOptions(options);

                        if (string.IsNullOrWhiteSpace(path))
                        {
                            break;
                        }

                        if (!Directory.Exists(path))
                        {
                            Console.WriteLine("Directory does not exist.");
                            continue;
                        }

                        if (!File.Exists(Path.Combine(path, game.ExeName + ".exe")))
                        {
                            Console.WriteLine("Correct game executable not found in this directory.");
                            continue;
                        }

                        CopyAssets(path, game is not TrackmaniaForeverGameInstallation);

                        break;
                    }

                    Console.WriteLine($": {arg} {(updatedAssets ? "(already updated)" : "")}");
                    continue;
            }

            if (!configProps.TryGetValue(argLower, out var confProp))
            {
                throw new ConsoleFailException("Unknown argument: " + arg);
            }

            if (confProp.PropertyType == typeof(string))
            {
                if (!argEnumerator.MoveNext())
                {
                    throw new ConsoleFailException("Missing value for config option " + arg);
                }

                var val = argEnumerator.Current;

                Console.WriteLine($": {arg} \"{val}\"");

                configPropsToSet.Add(confProp, val);
            }
            else
            {
                throw new ConsoleFailException($"Config option {arg} is not a string.");
            }
        }

        if (!anyAdditionalArguments)
        {
            Console.WriteLine("(none)");
        }

        return options;
    }

    private static void CopyAssets(string path, bool isManiaPlanet)
    {
        NadeoIni nadeoIni;

        try
        {
            nadeoIni = NadeoIni.Parse(Path.Combine(path, "Nadeo.ini"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse Nadeo.ini: {ex.Message}");
            return;
        }

        var assetsIdent = typeof(T).GetCustomAttribute<ToolAssetsAttribute>()?.Identifier ?? throw new Exception("Tool is missing ToolAssetsAttribute");
        var assetsDir = Path.Combine(rootPath, "Assets", "Tools", assetsIdent);

        var assetsIgnored = typeof(T).GetCustomAttributes<ToolAssetsIgnoreIngameAttribute>();

        foreach (var filePath in Directory.GetFiles(assetsDir, "*.*", SearchOption.AllDirectories))
        {
            var relativeFilePath = Path.GetRelativePath(assetsDir, filePath);

            if (!isManiaPlanet && relativeFilePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (isManiaPlanet && relativeFilePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (assetsIgnored.Any(a => relativeFilePath.StartsWith(a.Path)))
            {
                continue;
            }

            var updatedRelativePath = typeof(T).GetMethod(nameof(IHasAssets.RemapAssetRoute))?.Invoke(null, new object[] { relativeFilePath, isManiaPlanet }) as string ?? throw new Exception("Undefined file path");
            var finalPath = Path.Combine(nadeoIni.UserDataDir, updatedRelativePath);

            Console.WriteLine($"Copying {relativeFilePath} to {updatedRelativePath}...");

            var finalDir = Path.GetDirectoryName(finalPath);

            if (finalDir is not null)
            {
                Directory.CreateDirectory(finalDir);
            }

            File.Copy(filePath, finalPath, true);
        }

        Console.WriteLine("Copied!");
    }

    private static string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }
        else
        {
            if (type.GetGenericTypeDefinition() == typeof(NodeFile<>))
            {
                return type.GetGenericArguments()[0].Name;
            }
        }

        return "[unknown]";
    }

    private static Dictionary<string, PropertyInfo> GetConfigProps(out IList<PropertyInfo> configs)
    {
        Console.WriteLine("Retrieving possible config options...");

        var configurables = typeof(T).GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurable<>))
            .Select(x => x.GetGenericArguments()[0])
            .ToHashSet();

        configs = new List<PropertyInfo>();

        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.Name == "Config" && configurables.Contains(prop.PropertyType))
            {
                configs.Add(prop);
            }
        }

        if (configs.Count == 0)
        {
            Console.WriteLine("No config used by this tool.");
        }

        var configProps = new Dictionary<string, PropertyInfo>();

        foreach (var propConfig in configs)
        {
            var configPropArray = propConfig.PropertyType.GetProperties();

            if (configPropArray.Length == 0)
            {
                Console.WriteLine("No config properties found.");
            }

            foreach (var prop in configPropArray)
            {
                var nameLower = prop.Name.ToLowerInvariant();

                configProps[$"-c:{nameLower}"] = prop;
                configProps.Add($"-c:{propConfig.PropertyType.Name.ToLowerInvariant()}:{nameLower}", prop);
            }
        }

        return configProps;
    }
}
