using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class GameLoseUI : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    public Text LeftText;
    public Text PercentText;

    public Button backButton;
    public Button restartButton;

    // Start is called before the first frame update
    void Start()
    {
        backButton.onClick.AddListener(()=>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            UIController.Instance.HidePage(UIPageType.GameLoseUI);
            this.SendCommand(new LoadSceneCommand(Utils.SceneID.Game));
        });

        restartButton.onClick.AddListener(()=>{
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            UIController.Instance.HidePage(UIPageType.GameLoseUI);
            this.SendCommand(new LoadSceneCommand(Utils.SceneID.Game));
        });
    }

    void OnEnable()
    {
        this.GetSystem<AudioSystem>().PlaySingleSound("shibai");

        //this.GetUtility<SDKUtility>().ShowInterstitialAd();

        LeftText.text = $"{this.GetModel<RuntimeModel>().AllItems.Count}";
        
         var total = this.GetModel<RuntimeModel>().TotalBox.Value;

        var active = this.GetModel<RuntimeModel>().ActiveBoxes.Where(b => b.Type != BoxType.Advertisement).Count();
        var queueCount = this.GetModel<RuntimeModel>().BoxPool.Count;

        //Debug.Log($"left:{active} {queueCount} total:{total}");

        float percent = (float)(total - (active + queueCount)) / total;
        PercentText.text = $"{(int)(percent * 100)}%";
    }

    void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
