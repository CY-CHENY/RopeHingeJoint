using QFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

public class ProgressCtr : MonoBehaviour, IController
{
    public GameObject[] levelNodes;
    public GameObject[] frontNodes;

    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }


    void Start()
    {
        var model = this.GetModel<RuntimeModel>();
        model.CurrentLevel.RegisterWithInitValue(OnCurrentLevelChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private async void OnCurrentLevelChanged(int level)
    {
        BindableDictionary<int, LevelConfig> levelData = this.GetModel<RuntimeModel>().LevelLegoData;

        int name = (level - 1) % 5;
        foreach (var go in frontNodes)
        {
            go.SetActive(go.name.Equals(name.ToString()));
        }

        float defaultScale = 0.3f;
        int startLevel = (level - 1) / 5 + 1;
        startLevel = (startLevel - 1) * 5 + 1;
        for (int i = 0; i < levelNodes.Length; i++)
        {
            Debug.Log("startLevel:" + startLevel);
            LevelConfig data = levelData[startLevel];

            GameObject levelGo = levelNodes[i];

            var icon = levelGo.transform.Find("icon");

            if (((level - 1) % 5) == i)
            {
                icon.LocalScale(new Vector3(defaultScale * 1.3f, defaultScale * 1.3f, defaultScale * 1.3f));

            }
            else
            {
                icon.LocalScale(new Vector3(defaultScale, defaultScale, defaultScale));
            }

            var obj = await this.GetSystem<IAddressableSystem>().LoadAssetAsync<Sprite>(data.iconPath);
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                icon.GetComponent<SpriteRenderer>().sprite = obj.Result.Instantiate();
            }

            Transform text = levelGo.transform.Find("text");
            if (text != null)
            {
                text.GetComponent<TextMesh>().text = startLevel.ToString();
            }

            levelNodes[i].transform.Find("bgEnable").gameObject.SetActive(startLevel <= level);
            levelNodes[i].transform.Find("bgDisable").gameObject.SetActive(startLevel > level);
            startLevel++;
        }
    }

}
