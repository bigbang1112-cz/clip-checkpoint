using GBX.NET;

namespace GbxToolAPI;

public record NodeFile<T>(T Node, string? FileName = null, bool IsManiaPlanet = false) where T : Node
{
    public static explicit operator T(NodeFile<T> nodeFile)
    {
        return nodeFile.Node;
    }
}
