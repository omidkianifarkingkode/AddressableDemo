using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [SerializeField] PopupBase[] _preloadPopups;
    private readonly Dictionary<string, PopupBase> _popups = new();

    private void Awake()
    {
        Instance = this;

        foreach (var popup in _preloadPopups)
        {
            _popups[popup.PopupId] = popup;
            popup.gameObject.SetActive(false);
        }
    }

    public void OpenPopup(string popupId, PopupBase popupPrefab = default) 
    {
        if (_popups.TryGetValue(popupId, out var cachedPopup)) 
        {
            cachedPopup.gameObject.SetActive(true);

            return;
        }

        if(popupPrefab != null) 
        {
            var popupInstance = Instantiate(popupPrefab, transform);
            _popups[popupId] = popupInstance;
        }
    }
}
