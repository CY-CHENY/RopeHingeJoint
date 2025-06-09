using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QFramework;
using TTSDK;
using UnityEngine;
using UnityEngine.UI;

public class MainrankingItem : RecyclingListViewItem,IController
{
    public List<Sprite> spr_bg;
    public Image img_bg;
    public RawImage rimg_head;
    public Text txt_name;
    public Text txt_passLevel;
    public Text txt_rank;
    public Texture placeholderAvatar;
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    public void InitData(TTRank.RankResItem data,int rowIndex)
    {
        Debug.Log("---InitData----  " +rowIndex);
        img_bg.sprite = spr_bg[rowIndex < 3 ? rowIndex : 3];
        txt_passLevel.text = data.Value;
        txt_rank.text = rowIndex < 3 ? "" : rowIndex.ToString();
        txt_name.text = data.Nickname;
        DownloadAvater(data.UserImg);
    }

    public void InitMyData(TTRank.SelfRankResItem data)
    {
        Debug.Log("my data.Rank" +data.Rank);
        if (data.Rank - 1 < 0) data.Rank = 0; 
        img_bg.sprite = spr_bg[data.Rank < 4 ? data.Rank-1 : 3];
        txt_passLevel.text = data.Item.Value;
        txt_rank.text = data.Rank < 4 ? "" : data.Rank.ToString();
        txt_name.text = data.Item.Nickname;
        DownloadAvater(data.Item.UserImg);
    }

    void DownloadAvater(string url)
    {
        var downloadSystem = this.GetSystem<AvatarDownloadSystem>();
        downloadSystem.Download(url, (downloadedTexture) =>
        {
            rimg_head.texture = downloadedTexture;
        }, () =>
        {
            rimg_head.texture = placeholderAvatar;
        });
    }
}
