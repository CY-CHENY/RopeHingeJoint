using System.Collections;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneLoadingCtrl : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return TripleGame.Interface;
    }

    void Start()
    {
        this.GetModel<IGameModel>().SceneLoading.Value = true;
        this.GetModel<IGameModel>().SceneLoaded.Value = false;
        CoroutineController.Instance.StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        this.GetModel<IGameModel>().SceneLoaded.Value = false;
        this.GetModel<IGameModel>().SceneLoading.Value = true;
        UIController.Instance.HidePageByLevel(UILevelType.UIPage);
        UIController.Instance.HidePageByLevel(UILevelType.Main);
        AsyncOperation async = SceneManager.LoadSceneAsync((int)this.GetModel<IGameModel>().LoadingTargetSceneID.Value);
        async.allowSceneActivation = false;

        while (!async.isDone && async.progress < 0.9f)
        {
            yield return null;
        }

        this.GetModel<IGameModel>().SceneLoaded.Value = true;
        this.GetModel<IGameModel>().SceneLoading.Value = false;

        async.allowSceneActivation = true;
    }
}