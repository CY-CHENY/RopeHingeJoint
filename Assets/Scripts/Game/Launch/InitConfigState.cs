using QFramework;

public class InitConfigState : AbstractState<LaunchStates, Launch>, IController
{
    public InitConfigState(FSM<LaunchStates> fsm, Launch target) : base(fsm, target)
    {
    }

    protected override async void OnEnter()
    {
        await this.GetSystem<ConfigSystem>().LoadConfig();
        mFSM.ChangeState(LaunchStates.InitGameConfig);
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}