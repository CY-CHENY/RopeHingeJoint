using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class FuHuoUI : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    public Button closeButton;
    public Button fuhuoButton;

    // Start is called before the first frame update
    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            UIController.Instance.HidePage(UIPageType.FuHuoUI);
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.GameLoseUI, UILevelType.UIPage));
        });

        fuhuoButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.SendCommand(new FuHuoCommand());
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
