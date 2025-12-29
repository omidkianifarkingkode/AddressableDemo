using UnityEngine;

public class PopupBase : MonoBehaviour
{
    // Base class for popups; can be extended with common functionality if needed
    public string PopupId;

    public void Close() 
    {
        gameObject.SetActive(false);
    }
}
