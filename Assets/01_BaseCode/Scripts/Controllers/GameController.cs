using MoreMountains.NiceVibrations;
using UnityEngine;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif


public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public MoneyEffectController moneyEffectController;
    public UseProfile useProfile;
    public DataContain dataContain;
    public MusicManager musicManager;
    public AdmobAds admobAds;
    public AnalyticsController AnalyticsController;
    public IapController iapController;
    [HideInInspector] public SceneType currentScene;

    protected void Awake()
    {
        Instance = this;
        Init();
        DontDestroyOnLoad(this);

        //GameController.Instance.useProfile.IsRemoveAds = true;


#if UNITY_IOS
    if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == 
    ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
    {

        ATTrackingStatusBinding.RequestAuthorizationTracking();

    }

#endif
    }

    private void Start()
    {
        musicManager.PlayBGMusic();
    }

    public void Init()
    {
        //Application.targetFrameRate = 60;
        //useProfile.IsRemoveAds = true;
        useProfile.CurrentLevelPlay = UseProfile.CurrentLevel;
        admobAds.Init();
        musicManager.Init();
        iapController.Init();
        MMVibrationManager.SetHapticsActive(useProfile.OnVibration);
        // GameController.Instance.admobAds.ShowBanner();
    }

    public void LoadScene(string sceneName)
    {
        Initiate.Fade(sceneName, Color.black, 2f);
    }

    public static void SetUserProperties()
    {
        if (UseProfile.IsFirstTimeInstall)
        {
            UseProfile.FirstTimeOpenGame = UnbiasedTime.Instance.Now();
            UseProfile.LastTimeOpenGame = UseProfile.FirstTimeOpenGame;
            UseProfile.IsFirstTimeInstall = false;
        }

        var lastTimeOpen = UseProfile.LastTimeOpenGame;
        UseProfile.RetentionD = (UseProfile.FirstTimeOpenGame - UnbiasedTime.Instance.Now()).Days;

        var dayPlayerd = (TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()) -
                          TimeManager.ParseTimeStartDay(UseProfile.LastTimeOpenGame)).Days;
        if (dayPlayerd >= 1)
        {
            UseProfile.LastTimeOpenGame = UnbiasedTime.Instance.Now();
            UseProfile.DaysPlayed++;
        }

        AnalyticsController.SetUserProperties();
    }
}

public enum SceneType
{
    StartLoading = 0,
    MainHome = 1,
    GamePlay = 2
}