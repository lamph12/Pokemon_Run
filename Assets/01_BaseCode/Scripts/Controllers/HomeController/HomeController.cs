public class HomeController : Singleton<HomeController>
{
    public HomeScene homeScene;

    private void Start()
    {
        homeScene.Init();
    }

    protected override void OnAwake()
    {
        GameController.Instance.currentScene = SceneType.MainHome;
    }
}