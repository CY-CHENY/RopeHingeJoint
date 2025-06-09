using QFramework;
using UnityEngine;

public class BaseController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}