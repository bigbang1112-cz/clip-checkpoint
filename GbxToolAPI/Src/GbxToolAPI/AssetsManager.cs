using System.Reflection;
using System.Text;

namespace GbxToolAPI;

public static class AssetsManager<TTool> where TTool : ITool
{
    private static Func<string, Task<byte[]>>? ExternalRetrieve { get; set; }

    internal static bool RunsViaConsole { get; set; }

    public static async ValueTask<T> GetFromYmlAsync<T>(string path)
    {
        var toolAssetsIdentifier = typeof(TTool).GetCustomAttribute<ToolAssetsAttribute>()?.Identifier;

        if (toolAssetsIdentifier is null)
        {
            throw new NotSupportedException($"Assets are not supported for {typeof(TTool).Name}");
        }

        var entryAss = Assembly.GetEntryAssembly();

        if (!RunsViaConsole)
        {
            if (ExternalRetrieve is null)
            {
                throw new Exception("ExternalRetrieve needs to be set.");
            }

            // toolAssetsIdentifier is not even required
            var id = typeof(TTool).Assembly.GetName().Name ?? throw new Exception("Tool requires an Id");
            var route = typeof(TTool).GetCustomAttribute<ToolRouteAttribute>()?.Route ?? RegexUtils.PascalCaseToKebabCase(id);

            var data = await ExternalRetrieve($"{route}/{path.Replace('\\', '/')}");
            var str = Encoding.UTF8.GetString(data);

            return Yml.Deserializer.Deserialize<T>(str);
        }

        var rootPath = Environment.ProcessPath is null ? "" : Path.GetDirectoryName(Environment.ProcessPath) ?? "";

        using var r = File.OpenText(Path.Combine(rootPath, "Assets", "Tools", toolAssetsIdentifier, path));
        return Yml.Deserializer.Deserialize<T>(r);
    }
}
