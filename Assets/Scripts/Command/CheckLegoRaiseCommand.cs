using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class CheckLegoRaiseCommand : AbstractCommand
{
    
    protected override void OnExecute()
    {
        var model = this.GetModel<RuntimeModel>();
        this.SendEvent<LegoRaiseEvent>();
    }
}
