using QFramework;

public class InitNetworkState : AbstractState<LaunchStates, Launch>, IController
{
    public InitNetworkState(FSM<LaunchStates> fsm, Launch target) : base(fsm, target)
    {
    }

    protected override void OnEnter()
    {
        this.GetSystem<IAddressableSystem>().SetUpdateInfo(() => { ChangeState();});
    }

    private void ChangeState()
    {
        mFSM.ChangeState(LaunchStates.InitUI);
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}