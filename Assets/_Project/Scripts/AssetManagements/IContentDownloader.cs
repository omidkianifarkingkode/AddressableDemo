using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

public interface IContentDownloader
{
    void GetDownloadSize(IEnumerable<string> keys, Action<long> onComplete);
    void GetDownloadSize(string key, Action<long> onComplete);

    AsyncOperationHandle DownloadDependencies(
        IEnumerable<string> keys,
        Action<float> onProgress = null,
        Action<bool, long> onComplete = null);

    AsyncOperationHandle DownloadDependencies(
        string key,
        Action<float> onProgress = null,
        Action<bool, long> onComplete = null);

    void GetDownloadSizeWithLabels(string label, Action<long> onComplete);
    void GetDownloadSizeWithLabels(IEnumerable<string> labels, Action<long> onComplete);

    AsyncOperationHandle DownloadDependenciesWithLabels(
        string label,
        Action<float> onProgress = null,
        Action<bool, long> onComplete = null);

    AsyncOperationHandle DownloadDependenciesWithLabels(
        IEnumerable<string> labels,
        Action<float> onProgress = null,
        Action<bool, long> onComplete = null);

    bool IsDownloaded(string key);
}