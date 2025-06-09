using System;
using Cysharp.Threading.Tasks;
using QFramework;

public class AssetsUpdateState : AbstractState<LaunchStates, Launch>, IController
{
    public AssetsUpdateState(FSM<LaunchStates> fsm, Launch target) : base(fsm, target)
    {
    }

    protected override async void OnEnter()
    {
        //CoroutineController.Instance.StartCoroutine(this.SendCommand(new InitYooAssetCommand()));
        //this.RegisterEvent<AssetsInitEndEvent>(OnAssetsInitEnd);
        this.RegisterEvent<FinishDownloadResEvent>(OnFinishDownloadRes);
        UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.DownloadResUI, UILevelType.UIPage,
            isLocal: true));

    }

    private void OnFinishDownloadRes(FinishDownloadResEvent e)
    {
        if (e.isFinish)
        {
            mFSM.ChangeState(LaunchStates.InitConfig);
        }
        else
        {
            mFSM.ChangeState(LaunchStates.ExitGameState);
        }
    }

    // private async void OnAssetsInitEnd(AssetsInitEndEvent e)
    // {
    //     if (e.isFinish)
    //     {
    //         mFSM.ChangeState(LaunchStates.InitConfig);
    //     }
    //     else
    //     {
    //         mFSM.ChangeState(LaunchStates.ExitGameState);
    //     }
    // }

    protected override void OnExit()
    {
        // this.UnRegisterEvent<AssetsInitEndEvent>(OnAssetsInitEnd);
        this.UnRegisterEvent<FinishDownloadResEvent>(OnFinishDownloadRes);
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}