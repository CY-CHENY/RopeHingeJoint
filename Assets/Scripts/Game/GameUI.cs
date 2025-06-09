using System.Linq;
using cfg;
using DG.Tweening;
using QFramework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Utils;

public class GameUI : MonoBehaviour, IController
{
    public Button pauseButton;
    public Button resetButton;
    public Button zoomInButton;
    public Button zoomOutButton;

    public GameObject[] levelTray;
    //5个等级Icon
    public Image[] levelIcon;
    //等级数
    public Text[] levelText;
    //等级进度条

    //当前关卡小图标
    public Slider PercenSlider;
    public Image SliderBack;
    public Image SliderForward;
    //进度半分比
    public Text PercentText;

    //剩余积木
    public Text LeftText;

    public Button AddSpareButton;
    public Button ClearButton;
    public Button SuperBoxButton;

    private int maxLevel = 0;

    public Image imgLight;

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    void Start()
    {
        pauseButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.SettingsUI));
        });

        resetButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.GetSystem<VibrateSystem>().VibrateShort();
            this.SendCommand<ResetModelCameraSizeCommand>();
        });

        zoomInButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.GetSystem<VibrateSystem>().VibrateShort();
            this.SendCommand(new ZoomModelCommand(true));
        });

        zoomOutButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.GetSystem<VibrateSystem>().VibrateShort();
            this.SendCommand(new ZoomModelCommand(false));
        });

        AddSpareButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.GetSystem<VibrateSystem>().VibrateShort();
            this.SendCommand(new AddSpareCommand());
        });

        ClearButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.GetSystem<VibrateSystem>().VibrateShort();
            this.SendCommand(new ClearCommand());
        });

        SuperBoxButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            this.GetSystem<VibrateSystem>().VibrateShort();
            this.SendCommand(new SuperBoxCommand());
        });

        // InitGameData();

        var model = this.GetModel<RuntimeModel>();
        model.AllItems.OnCountChanged.Register(AllItemsChanged).UnRegisterWhenGameObjectDestroyed(gameObject);

        model.CurrentLevel.RegisterWithInitValue(OnCurrentLevelChanged).UnRegisterWhenGameObjectDestroyed(gameObject);

        this.RegisterEvent<BrickObjectSpawnedEvent>(OnBrickObjectSpawned).UnRegisterWhenGameObjectDestroyed(gameObject);
        //this.RegisterEvent<ProcessFullBoxFinishEvent>(OnProcessFullBoxFinished).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<CreateStarEvent>(OnCreateStar).UnRegisterWhenGameObjectDestroyed(gameObject);

        LeftText.text = model.AllItems.Count.ToString();
        PercenSlider.value = 0;

        imgLight.transform.DORotate(new Vector3(0, 0, 360), 20f, RotateMode.FastBeyond360).SetLoops(-1);
    }

    private void OnCreateStar(CreateStarEvent evt)
    {
        GameObject star = this.GetSystem<ParticlesSystem>().GetXingXing();
        star.transform.position = Camera.main.WorldToScreenPoint(evt.pos);
        star.transform.SetParent(transform);

        Vector3 targetPosition = PercenSlider.transform.position;

        star.transform.DOMove(targetPosition, 0.6f).OnComplete(() =>
        {
            Destroy(star);
            OnProcessFullBoxFinished();
        });

        PercenSlider.transform.DOBlendableScaleBy(new Vector3(0.2f, 0.2f, 0.2f), 0.1f).SetDelay(0.6f);
        PercenSlider.transform.DOBlendableScaleBy(new Vector3(-0.2f, -0.2f, -0.2f), 0.1f).SetDelay(0.7f);
    }

    private void OnProcessFullBoxFinished()
    {
        var total = this.GetModel<RuntimeModel>().TotalBox.Value;

        var active = this.GetModel<RuntimeModel>().ActiveBoxes.Where(b => b.Type == BoxType.Normal).Count();
        var poolCount = this.GetModel<RuntimeModel>().BoxPool.Count;

        Debug.Log($"total:{total} ,active:{active} ,poolCount:{poolCount}");

        float percent = (float)(total - (active + poolCount)) / total;
        PercenSlider.value = percent;
        Debug.Log((int)(percent * 100));
        PercentText.text = $"{(int)(percent * 100)}%";
    }

    // private void OnProcessFullBoxFinished(ProcessFullBoxFinishEvent evt)
    // {
    //     var total = this.GetModel<RuntimeModel>().TotalBox.Value;

    //     var active = this.GetModel<RuntimeModel>().ActiveBoxes.Where(b => b.Type == BoxType.Normal).Count();
    //     var poolCount = this.GetModel<RuntimeModel>().BoxPool.Count;

    //     //Debug.Log($"left:{active} {queueCount} total:{total}");

    //     float percent = (float)(total - (active + poolCount)) / total;
    //     PercenSlider.value = percent;
    //     PercentText.text = $"{(int)(percent * 100)}%";
    // }

    private void AllItemsChanged(int obj)
    {
        LeftText.text = obj.ToString();
    }

    private async void OnCurrentLevelChanged(int currentLevel)
    {
        BindableDictionary<int, LevelConfig> levelData = this.GetModel<RuntimeModel>().LevelLegoData;

        foreach (int level in levelData.Keys)
        {
            LevelConfig data = levelData[level];
            Sprite spriteIcon = null;
            Sprite spriteGray = null;
            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(data.IconPath);
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                spriteIcon = obj.Result;
            }

            var obj2 = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(data.IconGrayPath);
            if (obj2.Status == AsyncOperationStatus.Succeeded)
            {
                spriteGray = obj2.Result;
            }

            // levelText[level - 1].text = level.ToString();
            if (level == currentLevel)
            {
                SetPercenSlider(spriteGray, spriteIcon);
            }

            if (level > maxLevel)
                maxLevel = level;
        }

        PercentText.text = "0%";
    }

    // private async void InitGameData()
    // {
    //     BindableDictionary<int, LevelConfig> levelData = this.GetModel<RuntimeModel>().LevelLegoData;
    //     int currentLevel = this.GetModel<RuntimeModel>().CurrentLevel.Value;

    //     foreach (int level in levelData.Keys)
    //     {
    //         LevelConfig data = levelData[level];
    //         Sprite spriteIcon = null;
    //         Sprite spriteGray = null;
    //         var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(data.iconPath);
    //         if (obj.Status == AsyncOperationStatus.Succeeded)
    //         {
    //             spriteIcon = obj.Result;
    //         }

    //         var obj2 = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(data.iconGrayPath);
    //         if (obj2.Status == AsyncOperationStatus.Succeeded)
    //         {
    //             spriteGray = obj2.Result;
    //         }

    //         // levelText[level - 1].text = level.ToString();
    //         if (level == currentLevel)
    //         {
    //             SetPercenSlider(spriteGray, spriteIcon);
    //         }

    //         if (level > maxLevel)
    //             maxLevel = level;
    //     }

    //     PercentText.text = "0%";
    // }

    private void SetPercenSlider(Sprite spriteBack, Sprite spriteForward)
    {
        Debug.Log("SetPercenSlider");
        PercenSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(spriteBack.rect.width, spriteBack.rect.height);
        SliderBack.sprite = spriteBack;
        SliderForward.sprite = spriteForward;
    }


    private void OnBrickObjectSpawned(BrickObjectSpawnedEvent evt)
    {
        PercenSlider.value = 0.0f;
        PercentText.text = "0%";
    }
}
