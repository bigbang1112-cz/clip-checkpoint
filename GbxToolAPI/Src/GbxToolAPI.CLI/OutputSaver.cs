using GBX.NET;
using System.Diagnostics;

namespace GbxToolAPI.CLI;

internal class OutputSaver
{
    private readonly object output;
    private readonly string outputPath;
    private readonly Type outputType;

    public OutputSaver(object output, string outputPath)
    {
        this.output = output;
        this.outputPath = outputPath;
        outputType = output.GetType();
    }

    public void Save()
    {
        var outputType = output.GetType();

        if (outputType.IsGenericType)
        {
            SaveGeneric();
            return;
        }
        
        switch (output)
        {
            case Node node: // have specific filename formats defined somewhere, perhaps in GbxToolAPI directly
                node.Save(GenerateNodeFileName(node));
                break;
            default:
                throw new Exception("Unknown output type");
        }
    }

    private void SaveGeneric()
    {
        var genDefType = outputType.GetGenericTypeDefinition();

        if (genDefType == typeof(NodeFile<>))
        {
            SaveNodeFile();
        }
        else
        {
            throw new Exception("Unknown output type");
        }
    }

    private void SaveNodeFile()
    {
        var node = default(Node);
        var fileName = default(string);

        foreach (var prop in outputType.GetProperties())
        {
            switch (prop.Name)
            {
                case nameof(NodeFile<Node>.Node):
                    node = prop.GetValue(output) as Node;
                    break;
                case nameof(NodeFile<Node>.FileName):
                    fileName = prop.GetValue(output) as string ?? "";
                    break;
            }
        }

        if (node is null)
        {
            return; // maybe should throw?
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = GenerateNodeFileName(node);
        }

        var savePath = Path.Combine(outputPath, fileName);
        var saveDirPath = Path.GetDirectoryName(savePath);

        Console.WriteLine($"Output path: {saveDirPath}");
        Console.WriteLine($"Saving as {Path.GetFileName(fileName)}...");

        if (saveDirPath is not null)
        {
            Directory.CreateDirectory(saveDirPath);
        }

        var watch = Stopwatch.StartNew();

        node.Save(savePath); // temporary discard of the path info

        Console.WriteLine($"Saved. ({watch.Elapsed.TotalMilliseconds}ms)");
    }

    private static string GenerateNodeFileName(Node node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        var extension = NodeManager.GetGbxExtensions(node.Id).FirstOrDefault();

        return $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Guid.NewGuid()}{(extension is null ? "" : "." + extension)}.Gbx";
    }
}
