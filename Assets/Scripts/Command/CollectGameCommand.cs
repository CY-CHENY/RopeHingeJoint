using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class CollectGameCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var model = this.GetModel<PlayerInfoModel>();
        model.IsCollectedGame.Value = true;
    }
}
