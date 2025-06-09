using QFramework;
using UnityEngine;

public class EnterGameState : AbstractState<LaunchStates, Launch>, IController
{
    public EnterGameState(FSM<LaunchStates> fsm, Launch target) : base(fsm, target)
    {
    }

    protected override void OnEnter()
    {
        UIController.Instance.HidePage(UIPageType.DownloadResUI);
        this.SendCommand(new PrepareGameDataCommand());
        if (this.GetModel<IGameModel>().FirstGame.Value)
        {
            this.GetModel<IGameModel>().FirstGame.Value = false;
        }
        this.SendCommand(new LoadSceneCommand(Utils.SceneID.Game));
        
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}