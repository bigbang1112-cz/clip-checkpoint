using GbxToolAPI.Client.Models;

namespace GbxToolAPI.Client.Services;

public interface IDynamicScriptService
{
    Task SpawnScriptAsync(string src, string id);
    Task SpawnScriptAsync(Script script);
    Task SpawnScriptsAsync(params Script[] scripts);
    Task DespawnScriptAsync(string id);
}
