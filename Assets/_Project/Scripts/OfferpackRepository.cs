using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class OfferpackRepository : MonoBehaviour
{
    public static OfferpackRepository Instance { get; private set; }

    [Header("Default offerpacks to preload (labels/ids)")]
    [SerializeField] private bool preloadDefault;
    [SerializeField]
    private List<string> defaultOfferpackIds = new()
    {
        "offerpack-bag",
        "offerpack-weapon"
    };

    [Header("Address format")]
    [Tooltip("If offerpack id is 'offerpack-bag' -> address becomes 'offerpack-bag/bundle-data.asset'")]
    [SerializeField] private string bundleDataSuffix = "/bundle-data.asset";

    public event Action<string, OfferpackBundleData> OfferpackReady;
    public event Action<string, string> OfferpackFailed; // (id, reason)

    private readonly Dictionary<string, OfferpackBundleData> _cache = new();
    private readonly HashSet<string> _loading = new();

    private readonly Dictionary<string, AsyncOperationHandle> _downloadHandles = new();
    private readonly Dictionary<string, AsyncOperationHandle<OfferpackBundleData>> _dataHandles = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (preloadDefault)
            PreloadDefault();
    }

    public void PreloadDefault()
    {
        foreach (var id in defaultOfferpackIds)
            LoadOfferpack(id);
    }

    public bool TryGet(string offerpackId, out OfferpackBundleData data)
        => _cache.TryGetValue(offerpackId, out data) && data != null;

    public void LoadOfferpack(string offerpackId)
    {
        if (string.IsNullOrWhiteSpace(offerpackId))
            return;

        if (_cache.ContainsKey(offerpackId))
        {
            // Already ready: fire callback again (useful for late subscribers)
            OfferpackReady?.Invoke(offerpackId, _cache[offerpackId]);
            return;
        }

        if (_loading.Contains(offerpackId))
            return;

        _loading.Add(offerpackId);

        // Step 1: download dependencies by label (your label == offerpackId)
        var downloadHandle = Addressables.DownloadDependenciesAsync(offerpackId, true);
        _downloadHandles[offerpackId] = downloadHandle;

        downloadHandle.Completed += op =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Fail(offerpackId, "DownloadDependencies failed");
                return;
            }

            // Step 2: load OfferpackBundleData from a predictable address
            var dataAddress = offerpackId + bundleDataSuffix;
            var dataHandle = Addressables.LoadAssetAsync<OfferpackBundleData>(dataAddress);
            _dataHandles[offerpackId] = dataHandle;

            dataHandle.Completed += dataOp =>
            {
                if (dataOp.Status != AsyncOperationStatus.Succeeded || dataOp.Result == null)
                {
                    Fail(offerpackId, $"LoadAsset failed: {dataAddress}");
                    return;
                }

                _cache[offerpackId] = dataOp.Result;
                _loading.Remove(offerpackId);

                OfferpackReady?.Invoke(offerpackId, dataOp.Result);
            };
        };
    }

    private void Fail(string offerpackId, string reason)
    {
        _loading.Remove(offerpackId);
        OfferpackFailed?.Invoke(offerpackId, reason);
    }

    /// <summary>
    /// Release one offerpack's loaded handles (memory accounting). Bundles remain in disk cache.
    /// Call when entering gameplay if you want.
    /// </summary>
    public void Release(string offerpackId)
    {
        _cache.Remove(offerpackId);
        _loading.Remove(offerpackId);

        if (_dataHandles.TryGetValue(offerpackId, out var dataHandle))
        {
            if (dataHandle.IsValid()) Addressables.Release(dataHandle);
            _dataHandles.Remove(offerpackId);
        }

        if (_downloadHandles.TryGetValue(offerpackId, out var downloadHandle))
        {
            if (downloadHandle.IsValid()) Addressables.Release(downloadHandle);
            _downloadHandles.Remove(offerpackId);
        }
    }

    public void ReleaseAll()
    {
        // iterate on a copy to avoid modifying while iterating
        var keys = new List<string>(_downloadHandles.Keys);
        foreach (var id in keys) Release(id);
    }
}
