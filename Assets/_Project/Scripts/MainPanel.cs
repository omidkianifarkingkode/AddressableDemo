using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.XR;

public sealed class MainPanel : MonoBehaviour
{
    [SerializeField] private Transform _offerpackContainer;
    [SerializeField] private OfferpackUIElement _templateOfferpack;

    private List<OfferpackUIElement> _offerpacks = new();

    private void Start()
    {
        foreach (var offerpack in LiveopsManager.Instance.AvailableOfferpackIds)
        {
            string id = offerpack; // Capture local copy
            AddressablesContentDownloader.Instance.GetDownloadSize(id, (size) =>
            {
                Debug.Log($"Offerpack '{id}' download size: {size.FormatBytes()}");
                if (size <= 0)
                {
                    OfferpackRepository.Instance.GetAsset(offerpack, 
                        onLoaded: (id, data) =>
                        {
                            if (data == null)
                                return;

                            var offerpackElement = Instantiate(_templateOfferpack, _offerpackContainer);
                            offerpackElement.gameObject.SetActive(true);
                            offerpackElement.SetData(data, () => OnOfferClicked(id, data));

                            _offerpacks.Add(offerpackElement);
                        }, 
                        onProgress: (id, progress) => { Debug.Log($"{id} -> {progress}"); });
                }
                else
                {
                    AddressablesContentDownloader.Instance.DownloadDependencies(offerpack,
                        onProgress: (progress) => { Debug.Log($"{offerpack} -> progress:{progress}"); },
                        onComplete: (done, size) => { Debug.Log($"{offerpack} -> done:{done}, size:{size}"); });
                }
            });



            
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OfferpackRepository.Instance?.UnloadAll();
        }
    }

    private void OnDestroy()
    {
        OfferpackRepository.Instance?.UnloadAll();
    }

    private void OnOfferClicked(string offerId, OfferpackBundleData data)
    {
        PopupManager.Instance.OpenPopup(data.popupPrefab.PopupId, data.popupPrefab);

        Debug.Log($"[MainPanel] Clicked offer: {offerId} -> call MenuManager (skipped).");
    }
}
