namespace GbxToolAPI;

public interface IHasAssets
{
    ValueTask LoadAssetsAsync();
    static abstract string RemapAssetRoute(string route, bool isManiaPlanet);
}
