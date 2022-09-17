using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartLoading : MonoBehaviour
{
    public Text txtLoading;
    public Slider progressBar;
    public SkeletonGraphic heroRun;
    public Transform first;
    public Transform last;

    private void Start()
    {
        Application.targetFrameRate = 60;
        progressBar.value = 0f;
        progressBar.maxValue = 1f;
        StartCoroutine(ChangeScene());
        StartCoroutine(LoadingText());
    }

    // Use this for initialization
    private IEnumerator ChangeScene()
    {
        //yield return new WaitForSeconds(2f);

        // we start loading the scene
        //scene_name = GameUtils.SceneName.HOME_SCENE;
        var _asyncOperation = SceneManager.LoadSceneAsync("GamePlay");
        //_asyncOperation.allowSceneActivation = false;
        //Debug.Log("_asyncOperation " + _asyncOperation.progress);
        //// while the scene loads, we assign its progress to a target that we'll use to fill the progress bar smoothly
        while (!_asyncOperation.isDone)
        {
            progressBar.value = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
            float val = first.position.x + (last.position.x - first.position.x) * progressBar.value;
            heroRun.transform.position = new Vector3(val, first.position.y, first.position.z);
            yield return null;
        }

        //// we switch to the new scene
        //_asyncOperation.allowSceneActivation = true;
    }

    private IEnumerator LoadingText()
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            txtLoading.text = "LOADING .";
            yield return wait;

            txtLoading.text = "LOADING ..";
            yield return wait;

            txtLoading.text = "LOADING ...";
            yield return wait;
        }
    }
}