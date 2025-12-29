using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public sealed class AddressablesAssetProvider<T> : IAssetProvider<T>, IDisposable where T : Object
{
    private struct PendingRequest
    {
        public Action<T> OnLoaded;
        public Action<float> OnProgress;
        public Action<string> OnFailed;
    }

    private readonly Dictionary<string, AsyncOperationHandle<T>> _handles = new();
    private readonly Dictionary<string, List<PendingRequest>> _pending = new();
    private readonly ILogger _logger;

    public AddressablesAssetProvider(ILogger logger)
    {
        _logger = logger ?? Debug.unityLogger;
    }

    public void Load(string key,
                     Action<T> onLoaded,
                     Action<float> onProgress = null,
                     Action<string> onFailed = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            onFailed?.Invoke("Key is null or empty.");
            return;
        }

        if (onLoaded == null)
        {
            onFailed?.Invoke("onLoaded callback is required.");
            return;
        }

        // 1. Already successfully cached?
        if (_handles.TryGetValue(key, out var cachedHandle) &&
            cachedHandle.IsValid() &&
            cachedHandle.IsDone &&
            cachedHandle.Status == AsyncOperationStatus.Succeeded)
        {
            onLoaded(cachedHandle.Result);
            onProgress?.Invoke(1f);
            return;
        }

        // 2. Already loading? Enqueue callbacks
        if (_pending.TryGetValue(key, out var pendingList))
        {
            pendingList.Add(new PendingRequest
            {
                OnLoaded = onLoaded,
                OnProgress = onProgress,
                OnFailed = onFailed
            });
            return;
        }

        // 3. Start new load
        var requests = new List<PendingRequest>
        {
            new() { OnLoaded = onLoaded, OnProgress = onProgress, OnFailed = onFailed }
        };
        _pending[key] = requests;

        AsyncOperationHandle<T> handle;
        try
        {
            handle = Addressables.LoadAssetAsync<T>(key);
            _handles[key] = handle;

            _logger.Log($"[AddressablesProvider] Started loading '{key}'");
        }
        catch (Exception ex)
        {
            string error = $"Exception starting load for '{key}': {ex.Message}";
            _logger.LogError("[AddressablesProvider]", error);
            FailAndClearPending(key, error);
            return;
        }

        // Report progress
        if (onProgress != null || HasAnyProgressCallback(key))
        {
            ReportProgressPeriodically(key, handle);
        }

        // Completion
        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _logger.Log($"[AddressablesProvider] Successfully loaded '{key}'");
                SucceedAndClearPending(key, op.Result);
            }
            else
            {
                string reason = op.OperationException != null
                    ? $"Failed to load '{key}': {op.OperationException.Message}"
                    : $"Failed to load '{key}' (status: {op.Status})";

                _logger.LogError("[AddressablesProvider]", reason);
                FailAndClearPending(key, reason);
                CleanupFailedHandle(key);
            }
        };
    }

    private bool HasAnyProgressCallback(string key)
    {
        return _pending.TryGetValue(key, out var list) &&
               list.Exists(r => r.OnProgress != null);
    }

    private async void ReportProgressPeriodically(string key, AsyncOperationHandle<T> handle)
    {
        while (handle.IsValid() && !handle.IsDone)
        {
            float progress = handle.PercentComplete;
            InvokeProgressForAllPending(key, progress);
            await System.Threading.Tasks.Task.Delay(50); // ~20 updates/sec
        }

        // Final progress
        if (handle.IsDone)
        {
            InvokeProgressForAllPending(key, handle.PercentComplete);
        }
    }

    private void InvokeProgressForAllPending(string key, float progress)
    {
        if (_pending.TryGetValue(key, out var requests))
        {
            foreach (var req in requests)
            {
                try { req.OnProgress?.Invoke(progress); }
                catch (Exception e) { _logger.LogException(e); }
            }
        }
    }

    public bool TryGetCached(string key, out T asset)
    {
        asset = null;
        if (string.IsNullOrWhiteSpace(key)) return false;

        if (_handles.TryGetValue(key, out var handle) &&
            handle.IsValid() &&
            handle.IsDone &&
            handle.Status == AsyncOperationStatus.Succeeded)
        {
            asset = handle.Result;
            return true;
        }

        return false;
    }

    public void Unload(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        _pending.Remove(key);

        if (_handles.TryGetValue(key, out var handle))
        {
            _handles.Remove(key);
            if (handle.IsValid())
            {
                _logger.Log($"[AddressablesProvider] Unloaded '{key}'");
                Addressables.Release(handle);
            }
        }
    }

    public void UnloadAll()
    {
        _pending.Clear();

        foreach (var handle in _handles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        _handles.Clear();
        _logger.Log("[AddressablesProvider] Unloaded all assets.");
    }

    public void Dispose() => UnloadAll();

    // === Private Helpers ===

    private void SucceedAndClearPending(string key, T result)
    {
        if (!_pending.TryGetValue(key, out var requests)) return;

        _pending.Remove(key);
        foreach (var req in requests)
        {
            try { req.OnLoaded?.Invoke(result); }
            catch (Exception e) { _logger.LogException(e); }
        }
    }

    private void FailAndClearPending(string key, string reason)
    {
        if (!_pending.TryGetValue(key, out var requests)) return;

        _pending.Remove(key);
        foreach (var req in requests)
        {
            try { req.OnFailed?.Invoke(reason); }
            catch (Exception e) { _logger.LogException(e); }
        }
    }

    private void CleanupFailedHandle(string key)
    {
        if (_handles.TryGetValue(key, out var handle))
        {
            _handles.Remove(key);
            if (handle.IsValid())
                Addressables.Release(handle);
        }
    }
}