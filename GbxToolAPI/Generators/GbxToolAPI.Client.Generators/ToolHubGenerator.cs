using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace GbxToolAPI.Client.Generators;

[Generator]
public class ToolHubGenerator : ISourceGenerator
{
    private const bool Debug = false;

    public void Initialize(GeneratorInitializationContext context)
    {
        if (Debug && !Debugger.IsAttached)
        {
            Debugger.Launch();
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out string? projectDir))
        {
            throw new Exception("build_property.projectdir not found");
        }

        
    }
}