using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(OfferpackBundleData))]
public class OfferpackBundleData : ScriptableObject
{
    public Sprite Icon;
    public Sprite[] items;
    public GameObject popupPrefab;
}
