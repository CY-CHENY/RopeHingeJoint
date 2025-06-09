using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    public static CoroutineRunner Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<CoroutineRunner>();
                if(_instance == null)
                {
                    GameObject obj = new GameObject("SDK_CoroutineRunner");
                    _instance = obj.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(obj);
                    obj.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            return _instance;
        }
    }

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}