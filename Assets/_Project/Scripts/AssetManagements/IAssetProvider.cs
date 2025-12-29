using System;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public interface IAssetProvider<T> where T : Object
{
    /// <summary>
    /// Loads an asset asynchronously.
    /// </summary>
    /// <param name="key">Addressables key/address</param>
    /// <param name="onLoaded">Called when asset is successfully loaded</param>
    /// <param name="onProgress">Optional: called repeatedly with progress (0 to 1)</param>
    /// <param name="onFailed">Optional: called if loading fails</param>
    void Load(
        string key,
        Action<T> onLoaded,
        Action<float> onProgress = null,
        Action<string> onFailed = null);

    /// <summary>
    /// Tries to get a cached (already loaded) asset without starting a new load.
    /// Returns true if the asset is available in cache.
    /// </summary>
    bool TryGetCached(string key, out T asset);

    /// <summary>
    /// Unloads the asset and releases its Addressables handle.
    /// </summary>
    void Unload(string key);

    /// <summary>
    /// Unloads all loaded assets.
    /// </summary>
    void UnloadAll();
}

public static class AssetProviderExtensions
{
    public static Task<T> LoadAsync<T>(this IAssetProvider<T> provider, string key) where T : Object
    {
        var tcs = new TaskCompletionSource<T>();

        provider.Load(
            key,
            asset =>
            {
                tcs.TrySetResult(asset);
            },
            onProgress: null,
            onFailed: error =>
            {
                tcs.TrySetException(new Exception(error));
            });

        return tcs.Task;
    }
}
