using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AddressablesUpdater : MonoBehaviour
{
    [SerializeField] private bool checkOnStart = true;

    private void Start()
    {
        if (checkOnStart)
            CheckForCatalogUpdates();
    }

    public async void CheckForCatalogUpdates()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates();
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            Debug.Log($"[AddressablesUpdater] {checkHandle.Result.Count} catalog(s) need updating.");

            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
            await updateHandle.Task;

            Debug.Log("[AddressablesUpdater] Catalogs updated successfully.");
        }
        else
        {
            Debug.Log("[AddressablesUpdater] All catalogs are up to date.");
        }

        Addressables.Release(checkHandle);
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Addressables/Clear Download Cache")]
    public static void ClearAddressablesCache()
    {
        if (Caching.ClearCache())
        {
            Debug.Log("[Addressables] Download cache cleared successfully.");
        }
        else
        {
            Debug.LogWarning("[Addressables] Failed to clear cache (may be in use).");
        }

        // Optional: Also clear Addressables internal cache
        Addressables.ClearResourceLocators();
        Debug.Log("[Addressables] Resource locators cleared.");
    }

    [MenuItem("Tools/Addressables/Check for Catalog Updates (Manual)")]
    public static void ManualCatalogCheck()
    {
        var updater = FindObjectOfType<AddressablesUpdater>();
        if (updater != null)
            updater.CheckForCatalogUpdates();
        else
            Debug.LogWarning("No AddressablesUpdater found in scene.");
    }
#endif
}