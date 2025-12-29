using System.Collections.Generic;
using UnityEngine;

public sealed class MainPanel : MonoBehaviour
{
    [SerializeField] private Transform _offerpackContainer;
    [SerializeField] private OfferpackUIElement _templateOfferpack;

    private List<OfferpackUIElement> _offerpacks = new();

    private void Start()
    {
        foreach (var offerpack in LiveopsManager.Instance.AvailableOfferpackIds)
        {
            OfferpackRepository.Instance.GetAsset(offerpack, (id, data) => 
            {
                if (data == null)
                    return;

                var offerpackElement = Instantiate(_templateOfferpack, _offerpackContainer);
                offerpackElement.gameObject.SetActive(true);
                offerpackElement.SetData(data, () => OnOfferClicked(id, data));

                _offerpacks.Add(offerpackElement);
            });
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OfferpackRepository.Instance.UnloadAll();
        }
    }

    private void OnDestroy()
    {
        OfferpackRepository.Instance.UnloadAll();
    }

    private void OnOfferClicked(string offerId, OfferpackBundleData data)
    {
        PopupManager.Instance.OpenPopup(data.popupPrefab.PopupId, data.popupPrefab);

        Debug.Log($"[MainPanel] Clicked offer: {offerId} -> call MenuManager (skipped).");
    }
}
