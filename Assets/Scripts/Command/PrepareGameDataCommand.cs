using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using QFramework;
using Utils;

public class PrepareGameDataCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var runtimeModel = this.GetModel<RuntimeModel>();
        // int currentLevel = runtimeModel.CurrentLevel.Value;

        // int minLevel = (currentLevel - 1) / 5 + 1;
        // minLevel = (minLevel - 1) * 5 + 1;
        // int maxLevel = minLevel + 4;

        // UnityEngine.Debug.Log($"min:{minLevel} max:{maxLevel}");
        //暂时写死 可优化
        for (int i = 1; i <= 10; i++)
        {
            UnityEngine.Debug.Log("add:" + i + "TbLevelConfig");
            runtimeModel.LevelLegoData.Add(i, ConfigSystem.GetTable().TbLevelConfig.Get(i));
        }
        UnityEngine.Debug.Log("PrepareGameData Finished");
        
    }
}