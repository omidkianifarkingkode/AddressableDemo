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
public class RepositoryBootstrapper<TRepo, TData> : MonoBehaviour
    where TRepo : AssetBundleRepository<TData>
    where TData : Object
{
    private static bool _isInitiated = false;

    private TRepo _repository;

    [Header("Repository Configuration")]
    [SerializeField] private string bundleAddressFormat = "{id}/bundle-data.asset";

    [Header("Preload on Start")]
    [SerializeField] private string[] preloadIds = new string[0];

    [Header("Logging")]
    [SerializeField] private bool logEnabled = true;
    [SerializeField] private Color logColor = new(0.7f, 0.85f, 1f);

    private void Awake()
    {
        // Singleton enforcement
        if (_isInitiated)
        {
            Destroy(gameObject);
            return;
        }

        _isInitiated = true;
        DontDestroyOnLoad(gameObject);

        _repository = (TRepo) Activator.CreateInstance(typeof(TRepo), bundleAddressFormat, logEnabled, logColor);
    }

    private void Start()
    {
        if (preloadIds.Length > 0)
        {
            _repository.PreloadAssets(preloadIds);
        }
    }

    private void OnDestroy()
    {
        if (_repository != null)
        {
            _repository.Dispose();
            _repository = null;
            Debug.Log($"[RepositoryHolder] {typeof(TRepo).Name} disposed.");
        }
    }
}