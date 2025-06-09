using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class PageMainUI : MonoBehaviour,IController
{
        
    public Button btn_player;
    public Button btn_collectGames;
    public Button btn_signIn;
    public Button btn_ranking;
    public Button btn_dailyChallenge;
    public Button btn_startGame;
    public Button btn_feedback;
    public Button btn_info;
    public Button btn_setting;
    public Text txt_level;

    private Button btn_closePlayerSecondUI;
    public GameObject playerSecondUI;
    private PlayerInfoModel userModel;
    private void Awake()
    {
        playerSecondUI.SetActive(false);
        btn_closePlayerSecondUI = playerSecondUI.GetComponent<Button>();
        userModel = this.GetModel<PlayerInfoModel>();
        btn_dailyChallenge.SetActive(false);//隐藏每日挑战
    }

    private void Start()
    {
        btn_collectGames.onClick.AddListener(() =>
        {
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.CollectGamesUI, UILevelType.UIPage));
        });
        btn_signIn.onClick.AddListener(() =>
        {
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.SignInUI,UILevelType.UIPage));
        });
        btn_ranking.onClick.AddListener(() =>
        {
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.MainRankingUI,UILevelType.UIPage));
        });
        btn_dailyChallenge.onClick.AddListener(() =>
        {
            
        });
        btn_startGame.onClick.AddListener(() =>
        {
            this.SendCommand(new LoadSceneCommand(Utils.SceneID.Game));
        });
        btn_feedback.onClick.AddListener(() =>
        {
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.FeedbackUI, UILevelType.UIPage));
        });
        btn_info.onClick.AddListener(() =>
        {
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.PlayerInfoUI, UILevelType.UIPage));
        });
        btn_setting.onClick.AddListener(() =>
        {
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.SettingsUI, UILevelType.UIPage));
        });
        btn_closePlayerSecondUI.onClick.AddListener(() =>
        {
            playerSecondUI.SetActive(false);
        });
        btn_player.onClick.AddListener(() =>
        {
            playerSecondUI.SetActive(true);
        });
        userModel.GetCollectedGameAward.Register((b) =>
        {
            btn_collectGames.SetActive(!b);
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
        
    }

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        var model = this.GetModel<RuntimeModel>();
        txt_level.text =$"第{model.CurrentLevel.Value}关" ;
        btn_collectGames.SetActive(!userModel.GetCollectedGameAward.Value);
        playerSecondUI.SetActive(false);
    }
    

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
