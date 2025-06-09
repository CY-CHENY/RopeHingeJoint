using System;
using QFramework;
using UnityEngine;

public class InitGameConfigState : AbstractState<LaunchStates, Launch>, IController
{
    public InitGameConfigState(FSM<LaunchStates> fsm, Launch target) : base(fsm, target)
    {
    }

    protected override void OnEnter()
    {
        InitSettingsInfo();
        ChangeState();
    }

    private void ChangeState()
    {
        mFSM.ChangeState(LaunchStates.EnterGame);
    }

    private void InitSettingsInfo()
    {
        this.SendCommand<SaveSettingsCommand>();

#if !UNITY_EDITOR && DOUYINMINIGAME && UNITY_WEBGL
      string platform = SDKMgr.InStance().PrintSystemInfo().platform;
        if (platform.IndexOf("ios", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Application.targetFrameRate = 30;
            Debug.Log("设置目标帧率:" + 30);
        }
#endif

    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}