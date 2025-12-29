using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class AssetBundleRepository<T> : IDisposable where T : Object
{
    public event Action<string, T> OnAssetReady;
    public event Action<string, float> OnAssetProgress; // (id, progress 0-1)
    public event Action<string, string> OnAssetFailed; // (id, reason)

    private readonly string _bundleAddressFormat = "{id}/bundle-data.asset";

    private readonly Dictionary<string, T> _cache = new();
    private readonly IAssetProvider<T> _assetProvider;

    private readonly bool _logEnabled;
    private readonly Color _logColor;
    private readonly ILogger _logger;
    private readonly string _logTag;

    protected AssetBundleRepository(string bundleAddressFormat, bool logEnable, Color logColor)
    {
        _logTag = $"[{GetType().Name}]";

        _bundleAddressFormat = bundleAddressFormat;
        _logEnabled = logEnable;
        _logColor = logColor;

        _logger = new ScopedLogger(Debug.unityLogger, GetType().Name)
        {
            logEnabled = _logEnabled,
            TagColor = _logColor
        };

        _assetProvider = new AddressablesAssetProvider<T>(_logger);
    }

    public void PreloadAssets(params string[] preloadAssets)
    {
        if (preloadAssets == null || preloadAssets.Length == 0)
            return;

        foreach (var id in preloadAssets)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                GetAsset(id);
            }
        }
    }

    /// <summary>
    /// Requests an asset by ID with optional callbacks.
    /// </summary>
    public void GetAsset(
        string id,
        Action<string, T> onLoaded = null,
        Action<string, float> onProgress = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            const string reason = "Invalid ID (null/empty/whitespace).";
            OnAssetFailed?.Invoke(id, reason);
            onLoaded?.Invoke(id, null);
            return;
        }

        // 1. Local cache hit
        if (_cache.TryGetValue(id, out var cached) && cached != null)
        {
            onProgress?.Invoke(id, 1f);
            onLoaded?.Invoke(id, cached);
            OnAssetReady?.Invoke(id, cached);
            return;
        }

        // 2. Provider cache hit
        string address = BuildAddress(id);
        if (_assetProvider.TryGetCached(address, out var providerCached) && providerCached != null)
        {
            _cache[id] = providerCached;
            onProgress?.Invoke(id, 1f);
            onLoaded?.Invoke(id, providerCached);
            OnAssetReady?.Invoke(id, providerCached);
            return;
        }

        // 3. Start loading
        _logger.Log(_logTag, $"Loading asset: {id} ({address})");

        _assetProvider.Load(
            address,
            asset =>
            {
                if (asset == null)
                {
                    string reason = $"Loaded null asset for '{address}'.";
                    _logger.LogWarning(_logTag, reason);
                    OnAssetFailed?.Invoke(id, reason);
                    onLoaded?.Invoke(id, null);
                    return;
                }

                _cache[id] = asset;
                _logger.Log(_logTag, $"Successfully loaded: {id}");
                onProgress?.Invoke(id, 1f);
                onLoaded?.Invoke(id, asset);
                OnAssetReady?.Invoke(id, asset);
            },
            progress =>
            {
                onProgress?.Invoke(id, progress);
                OnAssetProgress?.Invoke(id, progress);
            },
            error =>
            {
                _logger.LogError(_logTag, $"Failed to load '{id}': {error}");
                OnAssetFailed?.Invoke(id, error);
            });
    }

    /// <summary>
    /// Unloads a specific asset
    /// </summary>
    public void Unload(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;

        _cache.Remove(id);
        _assetProvider.Unload(BuildAddress(id));
        _logger.Log(_logTag, $"Unloaded: {id}");
    }

    /// <summary>
    /// Unloads all cached assets
    /// </summary>
    public void UnloadAll()
    {
        _cache.Clear();
        _assetProvider.UnloadAll();
        _logger.Log(_logTag, "Unloaded all assets.");
    }

    public bool IsLoaded(string id)
        => !string.IsNullOrWhiteSpace(id) && _cache.TryGetValue(id, out var data) && data != null;

    protected virtual string BuildAddress(string id)
        => _bundleAddressFormat.Replace("{id}", id);

    /// <summary>
    /// Releases all resources. Call when repository is no longer needed.
    /// </summary>
    public void Dispose()
    {
        UnloadAll();

        // Dispose provider if it supports it
        if (_assetProvider is IDisposable disposableProvider)
            disposableProvider.Dispose();

        _logger.Log(_logTag, "Repository disposed.");
    }
}