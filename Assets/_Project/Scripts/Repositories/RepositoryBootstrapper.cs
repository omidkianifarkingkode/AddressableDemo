using UnityEngine;
using System;
using Object = UnityEngine.Object;

/// <summary>
/// Generic MonoBehaviour holder for any AssetBundleRepository<T>.
/// Place this on a GameObject in your scene (e.g., "RepositoryManager").
/// </summary>
/// <typeparam name="TRepo">The repository type (e.g., OfferpackRepository)</typeparam>
/// <typeparam name="TData">The data type the repo manages (e.g., OfferpackBundleData)</typeparam>
[DisallowMultipleComponent]
public abstract class RepositoryBootstrapper<TRepo, TData> : MonoBehaviour
    where TRepo : AssetBundleRepository<TData>
    where TData : Object
{
    [Header("Repository Configuration")]
    [SerializeField] protected string bundleAddressFormat = "{id}/bundle-data.asset";

    [Header("Preload on Start")]
    [SerializeField] protected string[] preloadIds = new string[0];

    [Header("Logging")]
    [SerializeField] protected bool logEnabled = true;
    [SerializeField] protected Color logColor = new(0.7f, 0.85f, 1f);

    private void Awake()
    {
        var existingInstance = GetRepositoryInstance();

        if (existingInstance != null)
        {
            Debug.LogWarning($"[RepositoryBootstrapper] Duplicate bootstrapper for {typeof(TRepo).Name}. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        CreateRepository();
    }

    private void Start()
    {
        var repo = GetRepositoryInstance();

        if (repo != null && preloadIds.Length > 0)
        {
            repo.PreloadAssets(preloadIds);
        }
    }

    private void OnDestroy()
    {
        var repo = GetRepositoryInstance();
        if (repo != null)
        {
            repo.Dispose();

            Debug.Log($"[RepositoryBootstrapper] {typeof(TRepo).Name} disposed and singleton cleared.");
        }
    }

    protected abstract void CreateRepository();

    protected abstract TRepo GetRepositoryInstance();
}