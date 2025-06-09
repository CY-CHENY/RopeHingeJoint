using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using QFramework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Utils;
using static TTSDK.TTRank;

public class RankingListUI : MonoBehaviour, IController
{

    public Button backButton;
    public Button nextButton;

    public GameObject panelButtons;

    public GameObject rankPrefab;
    public GameObject content;
    //public GameObject panel;
    public ScrollRect scrollView;
    public GameObject MyRank;

    private string selfId;
    private int totalNum;
    // private int myRank;

    // private GameObject myRankItem;
    float originY = 0f;

    private List<GameObject> items;
    private bool bInit;
    private int curPage;
    private float offset = 10f;
    float itemHeight = 107f;

    private float panelHeight;

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    void Awake()
    {
        items = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //panelButtons.SetActive(false);
        nextButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.SendCommand<NextLevelCommand>();
        });

        backButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.SendCommand(new LoadSceneCommand(Utils.SceneID.Main));
            this.GetModel<RuntimeModel>().CurrentLevel.Value++;
        });


    }

    void OnEnable()
    {
        panelButtons.SetActive(false);
        bInit = false;
        originY = 0;
        originY -= offset;
        curPage = 1;
        totalNum = 0;
        panelHeight = scrollView.GetComponent<RectTransform>().sizeDelta.y;
        bCheckBottom = true;
        // SetButton(false);
        foreach (var obj in items)
        {
            Destroy(obj);
        }
        items.Clear();
        GetRankData(curPage, 10);

        Util.DelayExecuteWithSecond(1f, () =>
        {
            panelButtons.SetActive(true);
        });

        // #if UNITY_EDITOR
        //         SetButton(true);
        // #endif
    }

    // void GetMyRankAndTotalNum()
    // {
    //     // this.GetUtility<SDKUtility>().GetRankData("all", 1, 100, (ref RankData rankData) =>
    //     // {
    //     //     myRank = rankData.SelfItem.Rank;
    //     //     int totalNum = rankData.TotalNum;
    //     //     selfId = rankData.SelfUserInfo.OpenId;
    //     //     Debug.Log("myRank:" + myRank);
    //     //     //不在排行榜内 直接显示前6名
    //     //     if (myRank <= 1)
    //     //     {
    //     //         SelfRankResItem self = rankData.SelfItem;
    //     //         var matchedList = rankData.Items.Take(6).ToList();
    //     //         AddItems(matchedList, 1);
    //     //         content.transform.LocalPositionY(354);
    //     //         SetButton(true);
    //     //     }
    //     //     else if (myRank <= 100)
    //     //     {
    //     //         GetMatchedList(rankData);
    //     //     }
    //     //     else
    //     //     {
    //     //         GetRankData(Mathf.CeilToInt(myRank / 100.0f), 100);
    //     //     }
    //     // },
    //     // (errMsg) =>
    //     // {
    //     //     Debug.LogError(errMsg);
    //     //     SetButton(true);
    //     // }
    //     // );
    // }

    void GetRankData(int page, int num)
    {
        this.GetUtility<SDKUtility>().GetRankData("all", page, num, (ref RankData rankData) =>
       {
           Debug.Log(rankData);
           if (!bInit)
           {
               selfId = rankData.SelfUserInfo.OpenId;
               totalNum = rankData.TotalNum;
               if (rankData.SelfItem.Rank == 0)
               {
                   MyRank.SetActive(false);
                   panelHeight = 730;
                   scrollView.GetComponent<RectTransform>().sizeDelta = new Vector2(800, panelHeight);
                   content.GetComponent<RectTransform>().sizeDelta.Y(panelHeight);
               }
               else
               {
                   MyRank.SetActive(true);
                   SetItem(rankData.SelfItem.Item, MyRank, rankData.SelfItem.Rank);
               }
               bInit = true;
           }

           for (int i = 0; i < rankData.Items.Count; i++)
           {
               var itemObj = Instantiate(rankPrefab, content.transform, false);
               itemObj.SetActive(true);
               SetItem(rankData.Items[i], itemObj, (page - 1) * num + (i + 1));
               itemObj.LocalPositionY(originY);
               originY -= itemHeight;
               originY -= offset;

               items.Add(itemObj);
           }

           if (Mathf.Abs(originY) > content.GetComponent<RectTransform>().sizeDelta.y)
           {
               var originSize = content.GetComponent<RectTransform>().sizeDelta;
               content.GetComponent<RectTransform>().sizeDelta = new Vector2(originSize.x, Mathf.Abs(originY));
           }

       },
       (errMsg) =>
       {
           Debug.LogError(errMsg);
       }
       );
    }
    private bool bCheckBottom = true;
    void Update()
    {
        //Debug.Log(scrollView.verticalNormalizedPosition);
        if (bCheckBottom && items.Count < totalNum && scrollView.verticalNormalizedPosition <= 0)
        {
            bCheckBottom = false;
            GetRankData(++curPage, 10);
            Util.DelayExecuteWithSecond(1f, () => bCheckBottom = true);
        }
    }

    void SetItem(RankResItem rankData, GameObject item, int rank)
    {
       // Debug.Log($"{rankData.OpenId}-->{rankData.Nickname}-->{rankData.Value}-->{rankData.UserImg}");
        if (rankData.OpenId.Equals(selfId))
        {
            item.transform.Find("bg1")?.gameObject.SetActive(false);
            item.transform.Find("bg2")?.gameObject.SetActive(true);
        }
        else
        {
            item.transform.Find("bg1")?.gameObject.SetActive(true);
            item.transform.Find("bg2")?.gameObject.SetActive(false);
        }

        Transform name = item.transform.Find("Name");
        if (name != null)
        {
            name.GetComponent<Text>().text = rankData.Nickname;
        }
        Transform level = item.transform.Find("Level");
        if (level != null)
        {
            level.GetComponent<Text>().text = rankData.Value;
        }
        Transform tranRank = item.transform.Find("Rank");
        if (tranRank != null)
        {
            tranRank.GetComponent<Text>().text = rank.ToString();
        }
        //青铜 
        Transform text = item.transform.Find("Text");
        if (text != null)
        {
            int nlevel = int.Parse(rankData.Value);
            if (nlevel <= 5)
            {
                text.GetComponent<Text>().text = "青铜V";
            }
            else if (nlevel <= 10)
            {
                text.GetComponent<Text>().text = "青铜IV";
            }
            else if (nlevel <= 15)
            {
                text.GetComponent<Text>().text = "青铜III";
            }
            else if (nlevel <= 20)
            {
                text.GetComponent<Text>().text = "青铜II";
            }
            else if (nlevel <= 25)
            {
                text.GetComponent<Text>().text = "青铜I";
            }
            else if (nlevel <= 30)
            {
                text.GetComponent<Text>().text = "白银V";
            }
        }
        //icon
        Transform icon = item.transform.Find("Icon");
        //avatar
        Transform avatar = item.transform.Find("Avatar");
        if (avatar != null)
        {
            this.GetUtility<ImageDownloader>().Download(rankData.UserImg, (texture) =>
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                avatar.GetComponent<Image>().sprite = sprite;
            });
        }
    }

    // void GetMatchedList(RankData rankData)
    // {
    //     SelfRankResItem self = rankData.SelfItem;
    //     int count = Mathf.Min(rankData.Items.Count, 30);
    //     var matchedList = rankData.Items.Where(i => i.Value.Equals(self.Item.Value) && !i.OpenId.Equals(selfId)).Take(count).ToList();
    //     if (matchedList == null)
    //     {
    //         matchedList = rankData.Items.Where(i => !i.OpenId.Equals(selfId)).Take(count).ToList();
    //     }
    //     //把自己插入到第二位
    //     matchedList.Insert(1, self.Item);
    //     AddItems(matchedList, self.Rank - 1);
    //     StartCoroutine(MoveToRank());
    // }

    // void AddItems(List<RankResItem> items, int startRank)
    // {
    //     float offset = 10f;
    //     float itemHeight = 107f;
    //     float topY = -itemHeight / 2 - offset;
    //     int count = items.Count;

    //     float panelHeight = panel.GetComponent<RectTransform>().sizeDelta.y;
    //     float height = Mathf.Max(panelHeight, itemHeight * (count + 1) + offset * (count + 2));
    //     var originSize = content.GetComponent<RectTransform>().sizeDelta;
    //     content.GetComponent<RectTransform>().sizeDelta = new Vector2(originSize.x, height);
    //     //Debug.Log($"panelHeight:{panelHeight} height:{height}");
    //     content.transform.LocalPositionY(height - panelHeight + panelHeight / 2);

    //     for (int i = 0; i < count; i++)
    //     {
    //         var item = addItem(items[i], startRank);
    //         item.transform.LocalPositionY(topY);
    //         topY -= offset;
    //         topY -= itemHeight;
    //         if (items[i].OpenId.Equals(selfId))
    //         {
    //             myRankItem = item;
    //         }
    //         startRank++;
    //     }

    //     if (myRankItem != null)
    //     {
    //         originY = myRankItem.transform.localPosition.y;
    //         myRankItem.transform.LocalPositionY(topY);
    //         // topY -= offset;
    //         // topY -= itemHeight;
    //     }
    // }

    // private GameObject addItem(RankResItem itemData, int rank)
    // {
    //     var item = Instantiate(rankPrefab, content.transform, false);
    //     item.SetActive(true);

    //     if (itemData.OpenId.Equals(selfId))
    //     {
    //         item.transform.Find("bg1").gameObject.SetActive(false);
    //         item.transform.Find("bg2").gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         item.transform.Find("bg1").gameObject.SetActive(true);
    //         item.transform.Find("bg2").gameObject.SetActive(false);
    //     }

    //     Transform name = item.transform.Find("Name");
    //     if (name != null)
    //     {
    //         name.GetComponent<Text>().text = itemData.Nickname;
    //     }
    //     Transform level = item.transform.Find("Level");
    //     if (level != null)
    //     {
    //         level.GetComponent<Text>().text = itemData.Value;
    //     }
    //     Transform tranRank = item.transform.Find("Rank");
    //     if (tranRank != null)
    //     {
    //         tranRank.GetComponent<Text>().text = rank.ToString();
    //     }
    //     //青铜 
    //     Transform text = item.transform.Find("Text");
    //     if (text != null)
    //     {
    //         int nlevel = int.Parse(itemData.Value);
    //         if (nlevel <= 5)
    //         {
    //             text.GetComponent<Text>().text = "青铜V";
    //         }
    //         else if (nlevel <= 10)
    //         {
    //             text.GetComponent<Text>().text = "青铜IV";
    //         }
    //         else if (nlevel <= 15)
    //         {
    //             text.GetComponent<Text>().text = "青铜III";
    //         }
    //         else if (nlevel <= 20)
    //         {
    //             text.GetComponent<Text>().text = "青铜II";
    //         }
    //         else if (nlevel <= 25)
    //         {
    //             text.GetComponent<Text>().text = "青铜I";
    //         }
    //         else if (nlevel <= 30)
    //         {
    //             text.GetComponent<Text>().text = "白银V";
    //         }
    //     }
    //     //icon
    //     Transform icon = item.transform.Find("Icon");
    //     //avatar
    //     Transform avatar = item.transform.Find("Avatar");
    //     if (avatar != null)
    //     {
    //         this.GetUtility<ImageDownloader>().Download(itemData.UserImg, (texture) =>
    //         {
    //             Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    //             avatar.GetComponent<Image>().sprite = sprite;
    //         });
    //     }
    //     items.Add(item);
    //     return item;
    // }

    // private IEnumerator MoveToRank()
    // {
    //     yield return new WaitForSeconds(2.0f);
    //     myRankItem.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    //     myRankItem.transform.DOLocalMoveY(originY, 1f);
    //     myRankItem.AddComponent<SortingGroup>().sortingOrder = 1;
    //     myRankItem.transform.SetAsLastSibling();

    //     float offset = content.GetComponent<RectTransform>().sizeDelta.y - panel.GetComponent<RectTransform>().sizeDelta.y;
    //     Debug.Log("offset:" + offset);
    //     content.transform.DOBlendableMoveBy(new Vector3(0, -offset, 0), 1f);

    //     yield return new WaitForSeconds(1f);

    //     myRankItem.transform.localScale = Vector3.one;
    //     // SetButton(true);
    // }

    // void SetButton(bool bShow)
    // {
    //     panelButtons.SetActive(bShow);
    // }

}
