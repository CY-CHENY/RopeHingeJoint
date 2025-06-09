using System.Collections;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class LoadSceneCommand : AbstractCommand
{
    private SceneID sceneID;
    public LoadSceneCommand(SceneID sceneID)
    {
        this.sceneID = sceneID;
    }

    protected override void OnExecute()
    {
        if(this.GetModel<IGameModel>().SceneLoading.Value)
            return;
        
        this.GetModel<IGameModel>().LoadingTargetSceneID.Value = sceneID;
        SceneManager.LoadScene((int)SceneID.Loading, LoadSceneMode.Additive);

        CoroutineController.Instance.StartCoroutine(UnloadPreviousScene());
    }

    IEnumerator UnloadPreviousScene()
    {
        yield return new WaitUntil(()=>SceneManager.GetSceneByBuildIndex((int)SceneID.Loading).isLoaded);
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }
}