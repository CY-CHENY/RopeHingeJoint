using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackUI : MonoBehaviour,IController
{
    public Button btn_submit;
    public Button btn_close;
    public ToggleGroup toggleGroup;
    public InputField inputField;
    private bool isInit;
    void Start()
    {
        btn_close.onClick.AddListener(() =>
        {
            UIController.Instance.HidePage(UIPageType.FeedbackUI);
        });
        
        btn_submit.onClick.AddListener(() =>
        {
            GetSelectedToggle();
            this.SendCommand(new SubmitFeedbackCommand(){inputTxt = inputField.text,selectIdx = GetSelectedToggle()});
            UIController.Instance.HidePage(UIPageType.FeedbackUI);
        });
        
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener((isOn) => {
                if (isOn)  this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            });
        }
        
        inputField.onEndEdit.AddListener(OnInputEndEdit);
        isInit = true;
    }

    private void OnInputEndEdit(string arg0)
    {
        var filterService =this.GetUtility<IWordFilterService>();
        string txt= filterService.Filter(inputField.text);
        inputField.text = txt;
    }

    void OnEnable()
    {
        InitUI();
    }

    void InitUI()
    {
        if (!isInit) return;
        StartCoroutine(SetFirstToggleAsSelected());
        inputField.text = String.Empty;
    }
    

    IEnumerator SetFirstToggleAsSelected()
    {
        yield return null;
        var toggles = toggleGroup.GetComponentsInChildren<Toggle>(true);

        if (toggles.Length  > 0)
        {
            toggles[0].isOn = true;
        }
    }
    
    int GetSelectedToggle()
    {
        Toggle selectedToggle = toggleGroup.ActiveToggles().FirstOrDefault();
        int selectIdx = 0;
        if (selectedToggle != null)
        {
            selectIdx = int.Parse(selectedToggle.name);
        }
        else
        {
            Log.Debug("没有选择任何选项");
        }

        return selectIdx;
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}
