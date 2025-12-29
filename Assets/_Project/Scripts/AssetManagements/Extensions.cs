using System;
using System.Threading.Tasks;

public static class Extensions
{
    public static Task<T> LoadAsync<T>(this IAssetProvider<T> provider, string key) where T : UnityEngine.Object
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

    public static string FormatBytes(this long bytes)
    {
        if (bytes <= 0) return "Already available";
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int i = 0;
        decimal d = bytes;
        while (d >= 1024 && i < suffixes.Length - 1)
        {
            d /= 1024;
            i++;
        }
        return $"{d:n1} {suffixes[i]} to download";
    }
}