using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;

public class SignInCommandBase : AbstractCommand
{
    public enum SignInMethod
    {
        Normal,     // 普通签到
        Ad          // 看广告签到
    }
    
    protected SignInMethod Method { get; }
    
    protected SignInCommandBase(SignInMethod method)
    {
        Method = method;
    }
    protected override void OnExecute()
    {
        var model = this.GetModel<SignInModel>();
        if (IsTodaySigned(model))
        {
            this.SendEvent<SignInFailedEvent>();
            return;
        }
        UpdateSignInData(model);
        GiveRewards(model.signInDays.Value);
        this.SendEvent(new SignInSuccessEvent());//model.signInDays.Value
    }
    
    private bool IsTodaySigned(SignInModel model)
    {
        if (string.IsNullOrEmpty(model.lastSignInDate.Value))
            return false;
            
        var lastDate = DateTime.Parse(model.lastSignInDate.Value);
        return lastDate.Date == DateTime.Today;
    }
    private void UpdateSignInData(SignInModel model)
    {
        model.signInDays.Value++;
        model.signedToday.Value = true; 
        // 更新最后签到日期
        model.lastSignInDate.Value = DateTime.Today.ToString("yyyy-MM-dd");
    }

    protected virtual void GiveRewards(int rewardIdx)
    {
        
    }
}

public class NormalSignInCommand : SignInCommandBase
{
    public NormalSignInCommand() : base(SignInMethod.Normal) { }

    protected override void GiveRewards(int rewardIdx)
    {
        Log.Debug("NormalSignInCommand  "+Method+"  "+rewardIdx);
        var playerInfoModel = this.GetModel<PlayerInfoModel>();
        var rewardInfo = this.GetModel<SignInModel>().GetSignInData(rewardIdx);
        playerInfoModel.ChangePropAmount(rewardInfo.rewards);
    }
}

public class AdSignInCommand  : SignInCommandBase
{
    public AdSignInCommand () : base(SignInMethod.Ad) { }

    protected override void GiveRewards(int rewardIdx)
    {
        Log.Debug("AdSignInCommand  " + Method);
        var playerInfoModel = this.GetModel<PlayerInfoModel>();
        var rewardInfo = this.GetModel<SignInModel>().GetSignInData(rewardIdx);
        var doubleItem = rewardInfo.rewards.Select(item => new PropBase
        {  
            id = item.id,
            amount = item.amount*2,
        }).ToList();
        playerInfoModel.ChangePropAmount(doubleItem);
        
    }

    protected override void OnExecute()
    {
        this.GetUtility<SDKUtility>().ShowAd((ret) =>
        {
            Log.Debug("每日签到广告:" + ret);
            if (ret)
            {
                base.OnExecute();
            }
            else
            {
                Log.Debug("广告奖励发放失败");
            }

        });

    }
    
}