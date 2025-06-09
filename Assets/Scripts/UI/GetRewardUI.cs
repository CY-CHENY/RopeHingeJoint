using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class GetRewardUI : MonoBehaviour,IController
{
    public List<RewardItem> rewardItems;
    private bool isInit;
    public Button btn_close;
    private void Start()
    {
        btn_close.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            UIController.Instance.HidePage(UIPageType.GetRewardUI);
        });
        isInit = true;
        InitUI();
    }

    private void OnEnable()
    {
        InitUI();
    }

    void InitUI()
    {
        if (!isInit) return;
        var model = this.GetModel<PlayerInfoModel>();

        for (int i = 0; i < rewardItems.Count; i++)
        {
            if (model.showRewards.Count > i)
            {
                rewardItems[i].InitData(model.showRewards[i]);
            }
            else
            {
                rewardItems[i].HideItem();
            }
        }
    }
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
