using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ConfirmAlertType
{
    Double = 0,
    Single
}

public class InfoConfirmInfo
{
    public string title;
    public string content;
    public Action succ;
    public Action fail;
    public string confirmText;
    public string cancelText;
    public ConfirmAlertType type;

    public InfoConfirmInfo(string title = "Tips", string content = "", Action success = null, Action fail = null,
        string confirmText = "Confirm", string cancelText = "Cancel",
        ConfirmAlertType type = ConfirmAlertType.Double)
    {
        this.title = title;
        this.content = content;
        this.succ = success;
        this.fail = fail;
        this.confirmText = confirmText;
        this.cancelText = cancelText;
        this.type = type;
    }
}

public class InfoConfirmAlert : UIPanel
{
    public Text titleText;
    public Text contentText;
    public Button cancelButton;
    public Button confirmButton;
    public Text confirmText;
    public Text cancelText;

    private Action success;
    private Action fail;

    private Queue<InfoConfirmInfo> queue = new Queue<InfoConfirmInfo>();

    public override void InitData(object data)
    {
        InfoConfirmInfo info = data as InfoConfirmInfo;
        if (info == null)
            return;

        ShowWithText(info.title, info.content, info.succ, info.fail, info.confirmText, info.cancelText, info.type);
    }

    private void ShowWithText(string infoTitle, string infoContent, Action infoSucc, Action infoFail,
        string infoConfirmText, string infoCancelText, ConfirmAlertType infoType)
    {
        InfoConfirmInfo info = new InfoConfirmInfo(infoTitle, infoContent, infoSucc, infoFail, infoConfirmText,
            infoCancelText, infoType);
        queue.Enqueue(info);
        UpdateUI(queue.Dequeue());
    }

    private void UpdateUI(InfoConfirmInfo info)
    {
        gameObject.SetActive(true);
        contentText.text = info.content;
        this.success = info.succ;
        this.fail = info.fail;
        cancelButton.gameObject.SetActive(info.type == ConfirmAlertType.Double);
        this.confirmText.text = info.confirmText;
        this.cancelText.text = info.cancelText;
        titleText.text = info.title;
    }

    private void Start()
    {
        cancelButton.onClick.AddListener(() =>
        {
            fail?.Invoke();
            //playSound
            if (queue.Count == 0)
                gameObject.SetActive(false);
            else
                UpdateUI(queue.Dequeue());
        });

        confirmButton.onClick.AddListener(() =>
        {
            success?.Invoke();
            //playsound
            if (queue.Count == 0)
                gameObject.SetActive(false);
            else
                UpdateUI(queue.Dequeue());
        });
    }
}