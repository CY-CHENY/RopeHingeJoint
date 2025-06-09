using System;
using QFramework;
using UnityEngine;

public class InitUIState : AbstractState<LaunchStates, Launch>, IController
{
    public InitUIState(FSM<LaunchStates> fsm, Launch target) : base(fsm, target)
    {
    }

    protected override async void OnEnter()
    {
        SetResolution();
        await UIController.Instance.InitUI();
        ChangeState();
    }

    void SetResolution()
    {
        //目标计算宽高比
        float designWidth = 720f;
        float designHeight = 1440f;
        float targetAspect = designWidth / designHeight;

        float deviceAspect = (float)Screen.width / Screen.height;
        float aspect = Camera.main.aspect;

        int renderWidth, renderHeight;
        if (deviceAspect > targetAspect)
        {
            renderWidth = 720;
            renderHeight = Mathf.RoundToInt(720 / aspect);
        }
        else
        {
            renderHeight = 1440;
            renderWidth = Mathf.RoundToInt(1440 * aspect);
        }

        Screen.SetResolution(renderWidth, renderHeight, true);
    }

    protected override void OnExit()
    {
        //  Game.UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.LoadingUI, UILevelType.Prepare));
    }

    private void ChangeState()
    {
        mFSM.ChangeState(LaunchStates.AssetsUpdate);
    }

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }
}