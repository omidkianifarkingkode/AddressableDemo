using UnityEngine;

[CreateAssetMenu(fileName = nameof(OfferpackBundleData))]
public class OfferpackBundleData : ScriptableObject
{
    public string Title;
    public Sprite Icon;
    public Sprite[] items;
    public PopupBase popupPrefab;
}
