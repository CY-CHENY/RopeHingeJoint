using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
using Utils;

public class SignInModel : AbstractModel
{
    public BindableProperty<int> signInDays = new BindableProperty<int>();
    public BindableProperty<string> lastSignInDate = new BindableProperty<string>();
    public BindableProperty<bool> signedToday = new BindableProperty<bool>();
    protected override void OnInit()
    {
        var storage = this.GetUtility<PlayerPrefsStorage>();
#if UNITY_EDITOR
        //清空签到数据
        // storage.SaveString(nameof(lastSignInDate), "");
        // storage.SaveString(nameof(signedToday), "false");
        // storage.SaveInt(nameof(signInDays), 0);
#endif
        signedToday.Value = bool.Parse(storage.LoadString(nameof(signedToday), "false"));
        signedToday.Register((b) => { storage.SaveString(nameof(signedToday), b.ToString()); });
        signInDays.Value = storage.LoadInt(nameof(signInDays), 0);
        signInDays.Register((day) => { storage.SaveInt(nameof(signInDays), day); });
        lastSignInDate.Value = storage.LoadString(nameof(lastSignInDate), "");
        lastSignInDate.Register((date) => { storage.SaveString(nameof(lastSignInDate), date); });

        
    }

    public void CheckIsNewDay()
    {
        var curDay = DateTime.Today.ToString("yyyy-MM-dd");
        if (curDay != lastSignInDate.Value)
        {
            if (signInDays.Value < 7) signedToday.Value = false;
            if (signInDays.Value == 7)
            {
                signInDays.Value = 0;
                signedToday.Value = false;
            }
        }
    }
    
    void InitSignInConfig()
    {
        List<SignInData> data = new List<SignInData>();
        var singInConfig = ConfigSystem.GetTable().TbSingInConfig.DataList;
        for (int i = 0; i < singInConfig.Count; i++)
        {
            SignInData _data = new SignInData();
            _data.signInDay = singInConfig[i].Id;
            foreach (var v in singInConfig[i].Type)
            {
                PropBase propBase = new PropBase();
                propBase.id = v.Key;
                propBase.amount = v.Value;
                _data.rewards.Add(propBase);
            }
            data.Add(_data);
        }
        signInData = data;
    }

    public SignInData GetSignInData(int day)
    {
        if (signInData == null) InitSignInConfig();
        return signInData.Find(v => v.signInDay == day);
    }
    
    
    public class SignInData
    {
        public int signInDay;
        public List<PropBase> rewards = new List<PropBase>();
    }

     List<SignInData> signInData;
}