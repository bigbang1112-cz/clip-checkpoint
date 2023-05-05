using GbxToolAPI.Client.Models;
using GbxToolAPI.Client.Models.UtilImport;
using Microsoft.AspNetCore.Components;

namespace GbxToolAPI.Client;

public abstract class ToolComponentBase : ComponentBase, IAsyncDisposable
{
    [Inject]
    public required Blazored.LocalStorage.ISyncLocalStorageService SyncLocalStorage { get; set; }

    [Inject]
    public required Blazored.LocalStorage.ILocalStorageService LocalStorage { get; set; }

    [Inject]
    public required NavigationManager NavMgr { get; set; }

    [Parameter]
    [EditorRequired]
    public required string Route { get; set; }

    public string SelectedConfigName
    {
        get
        {
            var val = SyncLocalStorage.GetItemAsString($"Tool:{Route}:Config:Selected");

            if (string.IsNullOrEmpty(val))
            {
                return "Default";
            }

            return val;
        }
        set
        {
            SyncLocalStorage.SetItem($"Tool:{Route}:Config:Selected", value);
        }
    }

    [Parameter]
    [EditorRequired]
    public Dictionary<string, Config> Configs { get; set; } = new();

    [Parameter]
    public HashSet<GbxModel> GbxSelection { get; set; } = new();

    [Parameter]
    [EditorRequired]
    public IEnumerable<UtilImportType> ImportTypes { get; set; } = Enumerable.Empty<UtilImportType>();

    [Parameter]
    [EditorRequired]
    public string ProceedType { get; set; } = "selected";

    [Parameter]
    [EditorRequired]
    public Dictionary<string, string> QueryParameters { get; set; } = new();

    public ToolComponentBase()
    {
        
    }

    protected override Task OnParametersSetAsync()
    {
        QueryParameters = new Dictionary<string, string>();

        var query = new Uri(NavMgr.Uri).Query.TrimStart('?');
        var pairs = query.Split('&');

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');

            if (keyValue.Length == 0)
            {
                continue;
            }

            var key = Uri.UnescapeDataString(keyValue[0]).ToLowerInvariant();
            var value = keyValue.Length >= 2 ? Uri.UnescapeDataString(keyValue[1]).ToLowerInvariant() : "";
            QueryParameters[key] = value;
        }

        return base.OnParametersSetAsync();
    }

    public void UpdateConfig()
    {
        SyncLocalStorage.SetItem($"Tool:{Route}:Config", Configs.ToDictionary(x => x.Key, x => (object)x.Value));
    }

    public async Task UpdateConfigAsync()
    {
        await LocalStorage.SetItemAsync($"Tool:{Route}:Config", Configs.ToDictionary(x => x.Key, x => (object)x.Value));
    }
    
    private bool disposedValue = false; // To detect redundant calls

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose of any disposable resources used by the component asynchronously
            }

            disposedValue = true;
        }
        await ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }
}
