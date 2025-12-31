using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class AddressablesUpdater : MonoBehaviour
{
    [SerializeField] private bool checkOnStart = true;

    public static event Action OnCataloadersUpdated;

    private void Start()
    {
        if (checkOnStart)
        {
            CheckForCatalogUpdates();
        }
    }

    public void CheckForCatalogUpdates()
    {
        // Step 1: Check for catalog updates
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates();

        checkHandle.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (handle.Result != null && handle.Result.Count > 0)
                {
                    Debug.Log($"[AddressablesUpdater] {handle.Result.Count} catalog(s) need updating.");

                    // Step 2: Update the catalogs if needed
                    AsyncOperationHandle<List<IResourceLocator>> updateHandle =
                        Addressables.UpdateCatalogs(handle.Result);

                    updateHandle.Completed += updateOp =>
                    {
                        if (updateOp.Status == AsyncOperationStatus.Succeeded)
                        {
                            Debug.Log("[AddressablesUpdater] Catalogs updated successfully.");

                            OnCataloadersUpdated?.Invoke();
                        }
                        else
                        {
                            Debug.LogError("[AddressablesUpdater] Failed to update catalogs: " + updateOp.OperationException);
                        }

                        // Always release the update handle
                        Addressables.Release(updateOp);
                    };
                }
                else
                {
                    Debug.Log("[AddressablesUpdater] All catalogs are up to date.");
                }
            }
            else
            {
                Debug.LogError("[AddressablesUpdater] Failed to check for catalog updates: " + handle.OperationException);
            }

            // Always release the check handle
            Addressables.Release(handle);
        };
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

        Addressables.ClearResourceLocators();
        Debug.Log("[Addressables] Resource locators cleared.");
    }

    [MenuItem("Tools/Addressables/Check for Catalog Updates (Manual)")]
    public static void ManualCatalogCheck()
    {
        var updater = FindObjectOfType<AddressablesUpdater>();
        if (updater != null)
        {
            updater.CheckForCatalogUpdates();
        }
        else
        {
            Debug.LogWarning("No AddressablesUpdater found in scene.");
        }
    }
#endif
}