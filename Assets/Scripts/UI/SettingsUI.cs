using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using TTSDK;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour, IController
{
    public Button closeButton;
    public Slider soundSlider;
    public Slider vibrationSlider;
    public Button mainButton;
    public Button continueButton;

    private bool isInit = false;
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseBtnClicked);
        continueButton.onClick.AddListener(OnCloseBtnClicked);
        mainButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.SendCommand(new LoadSceneCommand(Utils.SceneID.Main));
        });
        isInit = true;
        UpdateUI();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    private void OnCloseBtnClicked()
    {
        this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
        UIController.Instance.HidePage(UIPageType.SettingsUI);
        SaveSettings();
    }

    void UpdateUI()
    {
        if (!isInit)
            return;
        continueButton.SetActive(SceneManager.GetActiveScene().name != "Main");
        mainButton.SetActive(SceneManager.GetActiveScene().name != "Main");
        
        var info = this.GetModel<ISettingsModel>();

        soundSlider.value = Convert.ToInt32(info.IsOnSound.Value);
        vibrationSlider.value = Convert.ToInt32(info.IsOnVibration.Value);
    }

    private SystemSettingsInfo GetCurrentInfo()
    {
        SystemSettingsInfo info = new SystemSettingsInfo();
        info.isOnSound = Convert.ToBoolean(soundSlider.value);
        info.isOnVibration = Convert.ToBoolean(vibrationSlider.value);
        return info;
    }

    public void SaveSettings(Action success = null)
    {
        this.GetModel<ISettingsModel>().SaveSystemInfo(GetCurrentInfo());
        // 切换语言
        this.SendCommand<SaveSettingsCommand>();
        success?.Invoke();
    }
    private int timer = 0;
    public void OnClickOpenID()
    {
        this.timer++;
        Debug.Log("timer:" + this.timer);
        if (this.timer >= 10)
        {
            this.timer = 0;
            TT.SetClipboardData(PlayerPrefs.GetString("openid"));
        }

    }

    void OnDisable()
    {
        
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
