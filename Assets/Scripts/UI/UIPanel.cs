using QFramework;
using UnityEngine;

public abstract class UIPanel : MonoBehaviour, IController
{
    public abstract void InitData(object data);
    
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}