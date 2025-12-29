using System;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[CoroutineRunner]");
                _instance = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    private static CoroutineRunner _instance;

    public void RunNextFrame(Action action)
    {
        StartCoroutine(NextFrameCoroutine(action));
    }

    private static System.Collections.IEnumerator NextFrameCoroutine(Action action)
    {
        yield return null;
        action?.Invoke();
    }
}