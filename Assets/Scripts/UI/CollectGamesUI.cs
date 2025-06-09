using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using TTSDK;
using UnityEngine;
using UnityEngine.UI;

public class CollectGamesUI : MonoBehaviour,IController
{
    public Button btn_close;
    public Button btn_reward;
    public Button btn_goTo;
    public Image img_lock;
    public List<Sprite> list_lockSprite;

    private bool isCollected => TT.IsCollected();
    private PlayerInfoModel model;
    void Awake()
    {
        model = this.GetModel<PlayerInfoModel>();
        model.IsCollectedGame.Register(OnCollectedChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
        model.GetCollectedGameAward.Register(OnGetCollectedAwardChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
       
    }

    private void OnGetCollectedAwardChanged(bool isGet)
    {
        if (isGet)
        {
            UIController.Instance.HidePage(UIPageType.CollectGamesUI);
        }
    }

    private void OnCollectedChanged(bool isCollect)
    {
        if (isCollect)
        {
            CommonTip.instance.Show("收藏成功");
            UpdateUI();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        btn_close.onClick.AddListener(() =>
        {
            UIController.Instance.HidePage(UIPageType.CollectGamesUI);
        });
        btn_goTo.onClick.AddListener(() =>
        {
            this.SendCommand<CollectGameCommand>();

            // try
            // {
            //     TT.Collect((b) =>
            //     {
            //         Log.Debug("TT.Collect "+ b);
            //     });
            //     TT.CancelCollection((b) =>
            //     {
            //         Log.Debug("TT.CancelCollection "+ b);
            //     });
            //     Log.Debug("TT.IsCollected "+TT.IsCollected());
            // }
            // catch (Exception e)
            // {
            //    Log.Debug("Exception "+e.Message);
            // }
            
            TT.ShowFavoriteGuide(TTFavorite.Style.TopBar);
           
            // TT.ShowRevisitGuide((b) =>
            // {
            //     if (b)
            //     {
            //         Log.Debug("成功调起复访引导弹窗");
            //         this.SendCommand<CollectGameCommand>();
            //     }
            //     else
            //     {
            //         Log.Debug("调用失败");
            //     }
            // });

        });
        btn_reward.onClick.AddListener(() =>
        {
            if (!model.IsCollectedGame.Value)
            {
                CommonTip.instance.Show("请收藏后领取奖励");
                return;
            }
            this.SendCommand<GetCollectGameAwardCommand>();
        });
        
    }

    private void OnEnable()
    {
        UpdateUI();
    }

     void UpdateUI()
    {
        img_lock.sprite = list_lockSprite[model.IsCollectedGame.Value?1:0];
        btn_reward.SetActive(!model.GetCollectedGameAward.Value);
    }

   
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
