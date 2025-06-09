using System;
using System.Diagnostics;
using QFramework;

public class GameLogicSystem : AbstractSystem
{
    protected override void OnInit()
    {
        this.RegisterEvent<ProcessFullBoxFinishEvent>(OnCreateBoxFinished);
    }

    private void OnCreateBoxFinished(ProcessFullBoxFinishEvent evt)
    {
        if (this.GetModel<RuntimeModel>().AllItems.Count <= 0)
        {
            this.GetModel<RuntimeModel>().GameWin.Value = true;
            UnityEngine.Debug.Log("成功");
            this.GetUtility<SDKUtility>().SetRankData(this.GetModel<RuntimeModel>().CurrentLevel.Value);
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.GameWinUI, UILevelType.UIPage));
        }
    }
}