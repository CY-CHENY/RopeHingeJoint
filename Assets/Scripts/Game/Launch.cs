using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public enum LaunchStates
{
    InitNetwork,
    InitConfig,
    InitUI,
    InitGameConfig,
    AssetsUpdate,
    EnterGame,
    ExitGameState
}

public class Launch : BaseController
{
    public FSM<LaunchStates> FSM = new FSM<LaunchStates>();

    void Start()
    {
        FSM.AddState(LaunchStates.InitNetwork, new InitNetworkState(FSM, this));
        FSM.AddState(LaunchStates.InitUI, new InitUIState(FSM, this));
        FSM.AddState(LaunchStates.AssetsUpdate, new AssetsUpdateState(FSM, this));
        FSM.AddState(LaunchStates.InitConfig, new InitConfigState(FSM, this));
        FSM.AddState(LaunchStates.InitGameConfig, new InitGameConfigState(FSM, this));
        FSM.AddState(LaunchStates.EnterGame, new EnterGameState(FSM, this));
        FSM.AddState(LaunchStates.ExitGameState, new ExitGameState(FSM, this));

        FSM.StartState(LaunchStates.InitNetwork);

        this.GetUtility<SDKUtility>().Init();
    }
}
