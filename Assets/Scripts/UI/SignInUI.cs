using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class SignInUI : MonoBehaviour,IController
{
    public Button btn_close;

    public Button btn_get;

    public Button btn_ad;

    public List<SignInItem> items;
    private bool isInit = false;
    // Start is called before the first frame update
    void Start()
    {
        btn_close.onClick.AddListener(() =>
        {
            UIController.Instance.HidePage(UIPageType.SignInUI);
        });
        btn_get.onClick.AddListener(() =>
        {
            this.SendCommand<NormalSignInCommand>();
        });
        btn_ad.onClick.AddListener(() =>
        {
            this.SendCommand<AdSignInCommand>();
        });

        this.RegisterEvent<SignInFailedEvent>(OnSignInFailed).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<SignInSuccessEvent>(OnSignInSuccess).UnRegisterWhenGameObjectDestroyed(gameObject);
        isInit = true;
        UpdateUI();
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    private void OnSignInSuccess(SignInSuccessEvent obj)
    {
        CommonTip.instance.Show("签到成功");
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (!isInit)
            return;
        var model = this.GetModel<SignInModel>();
        model.CheckIsNewDay();
        btn_ad.interactable = !model.signedToday.Value;
        btn_get.interactable = !model.signedToday.Value;

        for (int i = 0; i < items.Count; i++)
        { 
            items[i].Init(model.signInDays.Value > i,i+1);
        }
    }

    private void OnSignInFailed(SignInFailedEvent obj)
    {
        CommonTip.instance.Show("签到失败：今日已签到");
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
