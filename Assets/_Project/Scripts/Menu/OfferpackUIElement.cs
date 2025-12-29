using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferpackUIElement : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI text;

    public void SetData(OfferpackBundleData offerpack, Action onClick)
    {
        icon.sprite = offerpack.Icon;
        text.SetText(offerpack.Title);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }
}
