using UnityEngine;

/// <summary>
/// Bootstrapper for AddressablesContentDownloader.
/// Creates a singleton-like instance with ScopedLogger (colored tag).
/// Place this on a GameObject in your bootstrap scene (e.g., "Managers").
/// </summary>
[DisallowMultipleComponent]
public class ContentDownloaderBootstrapper : MonoBehaviour
{
    [Header("Logging")]
    [SerializeField] private bool logEnabled = true;
    [SerializeField] private Color logColor = new Color(0.8f, 0.6f, 1f); // Purple-ish

    private AddressablesContentDownloader _instace;

    private void Awake()
    {
        if (AddressablesContentDownloader.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Create scoped logger with tag
        var scopedLogger = new ScopedLogger(Debug.unityLogger, "[ContentDownloader]")
        {
            logEnabled = logEnabled,
            TagColor = logColor
        };

        // Create downloader with injected logger
        _instace = new AddressablesContentDownloader(scopedLogger);
    }
}