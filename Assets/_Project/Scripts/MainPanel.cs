using UnityEngine;
using UnityEngine.UI;

public sealed class MainPanel : MonoBehaviour
{
    [Header("Offer Buttons (hidden until ready)")]
    [SerializeField] private Button bagOfferButton;
    [SerializeField] private Image bagOfferIcon;

    [SerializeField] private Button weaponOfferButton;
    [SerializeField] private Image weaponOfferIcon;

    [Header("Offerpack IDs (labels)")]
    [SerializeField] private string bagOfferId = "offerpack-bag";
    [SerializeField] private string weaponOfferId = "offerpack-weapon";

    private void Awake()
    {
        SetOfferVisible(bagOfferButton, bagOfferIcon, false);
        SetOfferVisible(weaponOfferButton, weaponOfferIcon, false);

        bagOfferButton.onClick.AddListener(() => OnOfferClicked(bagOfferId));
        weaponOfferButton.onClick.AddListener(() => OnOfferClicked(weaponOfferId));
    }

    private void OnEnable()
    {
        OfferpackRepository.Instance.OfferpackReady += OnOfferpackReady;
        OfferpackRepository.Instance.OfferpackFailed += OnOfferpackFailed;

        // start preload (repo can also auto-preload; calling again is safe)
        OfferpackRepository.Instance.LoadOfferpack(bagOfferId);
        OfferpackRepository.Instance.LoadOfferpack(weaponOfferId);

        // If something was loaded before we subscribed:
        if (OfferpackRepository.Instance.TryGet(bagOfferId, out var bagData)) 
            ApplyOfferpack(bagOfferId, bagData);
        if (OfferpackRepository.Instance.TryGet(weaponOfferId, out var weaponData)) 
            ApplyOfferpack(weaponOfferId, weaponData);
    }

    private void OnDisable()
    {
        if (OfferpackRepository.Instance == null) return;
        OfferpackRepository.Instance.OfferpackReady -= OnOfferpackReady;
        OfferpackRepository.Instance.OfferpackFailed -= OnOfferpackFailed;
    }

    private void OnOfferpackReady(string id, OfferpackBundleData data)
    {
        ApplyOfferpack(id, data);
    }

    private void OnOfferpackFailed(string id, string reason)
    {
        // Keep hidden if failed
        if (id == bagOfferId) SetOfferVisible(bagOfferButton, bagOfferIcon, false);
        if (id == weaponOfferId) SetOfferVisible(weaponOfferButton, weaponOfferIcon, false);

        Debug.LogWarning($"[MainPanel] Offerpack failed: {id} ({reason})");
    }

    private void ApplyOfferpack(string id, OfferpackBundleData data)
    {
        if (id == bagOfferId)
        {
            bagOfferIcon.sprite = data.Icon;
            SetOfferVisible(bagOfferButton, bagOfferIcon, true);
        }
        else if (id == weaponOfferId)
        {
            weaponOfferIcon.sprite = data.Icon;
            SetOfferVisible(weaponOfferButton, weaponOfferIcon, true);
        }
    }

    private void SetOfferVisible(Button button, Image iconImage, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
            button.interactable = visible;
        }

        if (iconImage != null)
            iconImage.enabled = visible && iconImage.sprite != null;
    }

    private void OnOfferClicked(string offerId)
    {
        // For now: just call MenuManager (skipped)
        // Example later:
        // MenuManager.Instance.OpenOffer(offerId);

        Debug.Log($"[MainPanel] Clicked offer: {offerId} -> call MenuManager (skipped).");
    }
}
