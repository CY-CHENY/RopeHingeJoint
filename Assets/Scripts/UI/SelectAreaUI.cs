using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectAreaUI : MonoBehaviour,IController
{
    public Button btn_close;

    public GameObject item;

    public Transform parent;
    public ScrollRect scrollRect; 
    private void Awake()
    {
        item.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        btn_close.onClick.AddListener(() =>
        {
            UIController.Instance.HidePage(UIPageType.SelectAreaUI);
        });

       
    }

    private void OnEnable()
    {
        Init();
    }

    void Init()
    {
        for (int i = 0; i < 40; i++)
        {
            if (parent.childCount > i)
            {
                var itemGo = parent.GetChild(i).gameObject;
                itemGo.SetActive(true);
                InitItem(itemGo,i);
            }
            else
            {
                GameObject itemGo = Instantiate(item,parent);
                itemGo.SetActive(true);
                InitItem(itemGo,i);
            }
            
        }
        
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }

    private void InitItem(GameObject go,int idx)
    {
        go.transform.Find("txt_name").GetComponent<Text>().text = idx.ToString();
    }


    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
