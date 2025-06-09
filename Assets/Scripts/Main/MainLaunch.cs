using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class MainLaunch : MonoBehaviour,IController
{
    public Transform modelSpwan;

    //public Button btn_show;
    private int index = 1;
    void Start()
    {
        UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.MainUI, UILevelType.Main));
        
        // btn_show.onClick.AddListener(() =>
        // {
        //     index++;
        //     this.SendCommand(new SpawnMainUIBrickObjectCommand(modelSpwan.localPosition,index));
        // });
    }

    private void OnEnable()
    {
        this.SendCommand(new SpawnMainUIBrickObjectCommand(modelSpwan.localPosition,1));
    }

    public IArchitecture GetArchitecture()
    {
        return  TripleGame.Interface;
    }
}