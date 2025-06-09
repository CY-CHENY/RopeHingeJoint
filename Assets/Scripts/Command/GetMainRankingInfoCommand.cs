using System.Collections;
using System.Collections.Generic;
using QFramework;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;

public class GetMainRankingInfoCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        //获取排行榜信息
        var paramJson = new JsonData
        {
            ["relationType"] = "all",
            ["dataType"] = 0,
            ["rankType"] = "all",
            ["pageNum"] = 1,
            ["pageSize"] = 10,
            ["zoneId"] = "default",
        };
        Debug.Log($"GetImRankDataNew param:{paramJson.ToJson()}");
        TT.GetImRankData(paramJson, 
            (ref TTRank.RankData data) =>
            {
                Log.Debug(data.ToString());
                Log.Debug($"data: {data}");
                Log.Debug($"selfRank : {data.SelfItem.Rank}");
                Log.Debug($"data: {data.Items.Count}");
                for (var i = 0; i < data.Items.Count; i++)
                {
                  
                    Log.Debug($"item-->nickName :{data.Items[i].Nickname}, openId:{data.Items[i].OpenId} , UserImg: {data.Items[i].UserImg}");    
                }
                Log.Debug("GetImRankDataNew true");
                this.GetModel<MainRankingModel>().rankData = data;
                this.SendEvent<GetRankDataSuccessEvent>();
            }, msg => { 
                this.SendEvent<GetRankDataFailEvent>();
                Log.Debug("GetImRankData Fail " + msg); 
            });
    }
}
