using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class SubmitFeedbackCommand : AbstractCommand
{
    public string inputTxt;
    public int selectIdx;

    protected override void OnExecute()
    {
        CommonTip.instance.Show("意见提交成功");
        Log.Debug($"inputTxt = {inputTxt} selectIdx ={selectIdx}");
    }
}
