using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using QFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SignInItem : MonoBehaviour, IController
{
    private Image img_icon;
    private GameObject go_received;
    private Text txt_day;
    private Text txt_amount;
    private ConfigSystem configSystem;
    private SignInModel model;
    private GameObject lightObj;

    private void Awake()
    {
        if (transform.Find("txt_day")) txt_day = transform.Find("txt_day").GetComponent<Text>();
        if (transform.Find("img_icon")) img_icon = transform.Find("img_icon").GetComponent<Image>();
        if (transform.Find("txt_amount")) txt_amount = transform.Find("txt_amount").GetComponent<Text>();
        if (transform.Find("lightObj")) lightObj = transform.Find("lightObj").gameObject;
        go_received = transform.Find("go_received").gameObject;
        if(lightObj!=null)lightObj.transform.DORotate(new Vector3(0, 0, 360),20f, RotateMode.FastBeyond360).SetLoops(-1);
        configSystem = this.GetSystem<ConfigSystem>();
        model = this.GetModel<SignInModel>();
        img_icon?.SetActive(false);
    }

    public void Init(bool isSignIn, int dayIndex)
    {
        var signInData = model.GetSignInData(dayIndex);
        txt_day.text = $"第{dayIndex}天";
        if (signInData.rewards.Count == 1)
        {
            LoadIcon(img_icon,dayIndex);
            txt_amount.text = "x" + signInData.rewards[0].amount;
        }
        else
        {
            for (int i = 0; i < signInData.rewards.Count; i++)
            {
                InitPropInfo(transform.Find("items").GetChild(i), signInData.rewards[i]);
            }
        }

        go_received.SetActive(isSignIn);
    }

    void InitPropInfo(Transform trans, PropBase data)
    {
        trans.Find("txt_amount").GetComponent<Text>().text =  "x" + data.amount;
        LoadIcon(trans,data.id);
    }

    async void LoadIcon(Image img_icon,int dayIndex)
    {
        var signInData = model.GetSignInData(dayIndex);
        Sprite spr =await configSystem.GetPropSprite(signInData.rewards[0].id);
        img_icon.sprite = spr;
        img_icon.SetActive(true);
    }

    async void LoadIcon(Transform trans,int id)
    {
        var spr = await this.GetSystem<ConfigSystem>().GetPropSprite(id);
        trans.GetComponent<Image>().sprite = spr;
        trans.gameObject.SetActive(true);
    }


    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}