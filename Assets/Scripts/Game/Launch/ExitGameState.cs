using QFramework;
using UnityEngine;

public class ExitGameState : AbstractState<LaunchStates, Launch>, IController
{
    public ExitGameState(FSM<LaunchStates> fsm, Launch target) : base(fsm, target)
    {
    }

    protected override void OnEnter()
    {
        // InfoConfirmInfo info = new InfoConfirmInfo(content: "游戏初始化失败，是否退出游戏?",
        //     success: () => { Application.Quit(); }, type: ConfirmAlertType.Double);
        // UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.AssetsUpdateAlert, UILevelType.Alert, info));
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}