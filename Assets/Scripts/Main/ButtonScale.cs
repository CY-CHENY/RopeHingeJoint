using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
[DisallowMultipleComponent]
public class ButtonScale : MonoBehaviour,IPointerDownHandler, IPointerUpHandler, IController
{
    public float pressedScale = 0.9f;
    private Vector3 originalScale;
    
    void Start()
    {
        originalScale = transform.localScale;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
        transform.localScale = originalScale * pressedScale;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
