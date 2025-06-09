using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour,IController
{
    public Button btn_collect;
    public Button btn_main;
    public Button btn_decorate;
    public Button btn_lock;
    public enum MainUIPage
    {
        Collect,
        Main,
        Decorate,
    }
    private Dictionary<MainUIPage, List<GameObject>> bottom_pages;

    private MainUIPage prev_page;
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    private void Awake()
    {
        bottom_pages = new Dictionary<MainUIPage, List<GameObject>>()
        {
            {
                MainUIPage.Collect, new List<GameObject>()
                {
                    btn_collect.transform.Find("on").gameObject,
                    transform.Find("PageCollectUI").gameObject,
                }
            },
            {
                MainUIPage.Main, new List<GameObject>()
                {
                    btn_main.transform.Find("on").gameObject,
                    transform.Find("PageMainUI").gameObject,
                }
            },
            {
                MainUIPage.Decorate, new List<GameObject>()
                {
                    btn_decorate.transform.Find("on").gameObject,
                    transform.Find("PageDecorateUI").gameObject,
                }
            },
        };
    }

    private void Start()
    {
      
        btn_collect.onClick.AddListener(()=>SwitchPage(MainUIPage.Collect));
        btn_main.onClick.AddListener(()=>SwitchPage(MainUIPage.Main));
        btn_decorate.onClick.AddListener(()=>SwitchPage(MainUIPage.Decorate));
        btn_lock.onClick.AddListener(() =>
        {
            CommonTip.instance.Show("敬请期待");
        });
        SwitchPage(MainUIPage.Main);
    }

    private void SwitchPage(MainUIPage page)
    {
        if (prev_page == page)
        {
            return;
        } 
        foreach (var v in bottom_pages)
        {
            for (int i = 0; i < v.Value.Count; i++)
            {
                v.Value[i].SetActive(v.Key == page);
            }
        }

        switch (page)
        {
            case MainUIPage.Collect:
                transform.Find("PageCollectUI").GetComponent<PageCollectUI>().Init();
                break;
            case MainUIPage.Main:
                break;
            case MainUIPage.Decorate:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(page), page, null);
        }

        prev_page = page;
    }

}
