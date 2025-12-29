using System.Collections.Generic;
using UnityEngine;

public class LiveopsManager : MonoBehaviour
{
    public static LiveopsManager Instance { get; private set; }

    public IEnumerable<string> AvailableOfferpackIds => new[]
    {
        bagOfferId,
        weaponOfferId
    };

    public string bagOfferId = "offerpack-bag";
    public string weaponOfferId = "offerpack-weapon";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
