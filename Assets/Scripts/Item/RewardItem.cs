using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class RewardItem : MonoBehaviour,IController
{
    public Image img_icon;
    public Text txt_amount;

    public async void InitData(PropBase data)
    {
        gameObject.SetActive(false);
        var model = await this.GetSystem<ConfigSystem>().GetPropSprite(data.id);
        img_icon.sprite = model;
        txt_amount.text = "x"+data.amount;
        gameObject.SetActive(true);
    }

    public void HideItem()
    {
        gameObject.SetActive(false);
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
