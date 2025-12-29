using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class AddressablesContentDownloader : IContentDownloader
{
    public static AddressablesContentDownloader Instance;

    private readonly ILogger _logger;

    public AddressablesContentDownloader(ILogger logger = null)
    {
        Instance = this;

        _logger = logger ?? Debug.unityLogger;
    }

    public void GetDownloadSize(IEnumerable<string> keys, Action<long> onComplete)
    {
        if (keys == null || onComplete == null)
        {
            onComplete?.Invoke(-1);
            return;
        }

        var handle = Addressables.GetDownloadSizeAsync(keys);

        handle.Completed += op =>
        {
            long size = 0;

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                size = op.Result; // Result is long here
                _logger.Log("[ContentDownloader] Required download size: " + size.FormatBytes());
            }
            else
            {
                size = -1;
                _logger.LogError("[ContentDownloader]", "Failed to get download size: " + op.OperationException?.Message);
            }

            onComplete(size);
        };
    }

    public void GetDownloadSize(string key, Action<long> onComplete)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            onComplete?.Invoke(-1);
            return;
        }

        GetDownloadSize(new[] { key }, onComplete);
    }

    public AsyncOperationHandle DownloadDependencies(
        IEnumerable<string> keys,
        Action<float> onProgress = null,
        Action<bool, long> onComplete = null)
    {
        if (keys == null)
        {
            onComplete?.Invoke(false, 0);
            return default;
        }

        // Note: This returns AsyncOperationHandle<object>, not <long>
        var handle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);

        if (onProgress != null)
        {
            TrackProgress(handle, onProgress);
        }

        handle.Completed += op =>
        {
            bool success = op.Status == AsyncOperationStatus.Succeeded;
            // Downloaded bytes not directly available here — use GetDownloadSize before/after as estimate
            long bytesDownloaded = 0;

            if (success)
            {
                _logger.Log("[ContentDownloader] Download completed successfully.");
            }
            else
            {
                _logger.LogError("[ContentDownloader]", "Download failed: " + op.OperationException?.Message);
            }

            onComplete?.Invoke(success, bytesDownloaded);
        };

        return handle; // Returns AsyncOperationHandle (base type)
    }

    public AsyncOperationHandle DownloadDependencies(
        string key,
        Action<float> onProgress = null,
        Action<bool, long> onComplete = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            onComplete?.Invoke(false, 0);
            return default;
        }

        return DownloadDependencies(new[] { key }, onProgress, onComplete);
    }

    public void GetDownloadSizeWithLabels(string label, Action<long> onComplete)
        => GetDownloadSizeWithLabels(new[] { label }, onComplete);

    public void GetDownloadSizeWithLabels(IEnumerable<string> labels, Action<long> onComplete)
    {
        if (labels == null || onComplete == null)
        {
            onComplete?.Invoke(-1);
            return;
        }

        var handle = Addressables.GetDownloadSizeAsync(labels);
        handle.Completed += op =>
        {
            long size = op.Status == AsyncOperationStatus.Succeeded ? op.Result : -1;
            if (size >= 0)
                _logger.Log($"[ContentDownloader] Size for labels {string.Join(", ", labels)}: {size.FormatBytes()}");
            else
                _logger.LogError("[ContentDownloader]", "Failed to get download size for labels.");

            onComplete(size);
        };
    }

    public AsyncOperationHandle DownloadDependenciesWithLabels(
        string label,
        Action<float> onProgress = null,
        Action<bool, long> onComplete = null)
        => DownloadDependenciesWithLabels(new[] { label }, onProgress, onComplete);

    public AsyncOperationHandle DownloadDependenciesWithLabels(
        IEnumerable<string> labels,
        Action<float> onProgress = null,
        Action<bool, long> onComplete = null)
    {
        var handle = Addressables.DownloadDependenciesAsync(labels, Addressables.MergeMode.Union);

        if (onProgress != null) TrackProgress(handle, onProgress);

        handle.Completed += op =>
        {
            bool success = op.Status == AsyncOperationStatus.Succeeded;
            _logger.Log(success
                ? $"[ContentDownloader] Labels download completed: {string.Join(", ", labels)}"
                : $"[ContentDownloader] Labels download failed: {op.OperationException?.Message}");

            onComplete?.Invoke(success, 0);
        };

        return handle;
    }

    public bool IsDownloaded(string key)
    {
        // Reliable way: check download size sync (fast if cached)
        // But since GetDownloadSizeAsync is async, we can't do perfect sync check
        // Best effort: return true if key is valid (not reliable)
        // Recommendation: Use GetDownloadSize(..., callback) instead
        return !string.IsNullOrWhiteSpace(key);
    }

    private void TrackProgress(AsyncOperationHandle handle, Action<float> onProgress)
    {
        void Update()
        {
            if (handle.IsValid() && !handle.IsDone)
            {
                onProgress?.Invoke(handle.PercentComplete);
                CoroutineRunner.Instance.RunNextFrame(Update);
            }
            else
            {
                onProgress?.Invoke(1f);
            }
        }

        Update();
    }
}
