using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class GetCollectGameAwardCommand : AbstractCommand
{

    protected override void OnExecute()
    {
        var model = this.GetModel<PlayerInfoModel>();
        model.GetCollectGameAward();
    }
}
