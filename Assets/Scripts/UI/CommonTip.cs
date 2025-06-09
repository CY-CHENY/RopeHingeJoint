using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class CommonTip : MonoBehaviour
{
    public static CommonTip instance;
    private Tween fadeTween;
    public Text txt_tip;
    private CanvasGroup cg;
    private void Awake()
    {
        instance = this;
        cg = txt_tip.transform.parent.GetComponent<CanvasGroup>();
        cg.gameObject.SetActive(false);
    }

    public void Show(string tip)
    {
        cg.gameObject.SetActive(true);
        txt_tip.text = tip;
        CancelAndRestartFade();
    }
    
    public void StartFade()
    {
        // 先取消任何正在进行的动画
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill(); // 终止当前动画
        }

        cg.alpha = 1;

        // 开始新的淡入动画
        fadeTween = cg.DOFade(0f, 1).SetDelay(2)
            .OnComplete(() =>
            {
                cg.gameObject.SetActive(false);
            });
    }

    public void CancelAndRestartFade()
    {
        StartFade(); // 直接调用开始方法会先取消之前的动画
    }
}
