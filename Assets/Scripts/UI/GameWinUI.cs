using DG.Tweening;
using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GameWinUI : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    public Image backLight;
    public Image Icon;
    public Text textCount;
    public Button goButton;

    // Start is called before the first frame update
    void Start()
    {
        backLight.transform.DORotate(new Vector3(0, 0, 360), 5f, RotateMode.FastBeyond360).SetLoops(-1);
        goButton.onClick.AddListener(() =>
        {
            this.GetSystem<AudioSystem>().PlaySingleSound("dianji");
            UIController.Instance.HidePage(UIPageType.GameWinUI);
            UIController.Instance.ShowPage(new ShowPageInfo(UIPageType.RankingListUI, UILevelType.UIPage));
        });
    }

    async void OnEnable()
    {
        int level = this.GetModel<RuntimeModel>().CurrentLevel.Value;
        var data = this.GetModel<RuntimeModel>().LevelLegoData[level];

        var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(data.iconPath);
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            Icon.sprite = obj.Result.Instantiate();
            Icon.GetComponent<RectTransform>().sizeDelta = new Vector2(Icon.sprite.rect.width, Icon.sprite.rect.height);
        }

        this.GetSystem<AudioSystem>().PlaySingleSound("shengli");
    }

    void OnDisable()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
