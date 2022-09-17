using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueRate : BaseBox
{
    private const int MIN_API_LEVEL_REVIEW = 21;

    private static DialogueRate instance;
    [SerializeField] private ReviewInAppController reviewInAppController;


    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private List<Sprite> lstSprStar;
    [SerializeField] private List<Button> lstBtnStar;
    [SerializeField] private List<Image> lstImgStar;
    private int countStar;

    public static DialogueRate Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<DialogueRate>(PathPrefabs.RATE_GAME_BOX));
            instance.Init();
        }

        //ChickenDataManager.CountTillShowRate = 0;
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Init()
    {
        btnConfirm.onClick.AddListener(RateAction);
        btnClose.onClick.AddListener(CloseAction);
    }

    public void InitState()
    {
        for (var i = 0; i < lstBtnStar.Count; i++)
        {
            var index = i + 1;
            // lstBtnStar[i].onClick.AddListener(() => { ClickStar(index); });
            lstImgStar[i].sprite = lstSprStar[0];
        }

        countStar = 0;
    }

    public void ClickStar(int index)
    {
        countStar = index;
        for (var i = 0; i < lstImgStar.Count; i++) lstImgStar[i].sprite = lstSprStar[0];
        for (var i = 0; i < index; i++) lstImgStar[i].sprite = lstSprStar[1];
        //GameController.Instance.musicManager.Pla();
    }

    public void RateAction()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (countStar <= 0)
            return;
        if (countStar == 5)
        {
            UseProfile.CanShowRate = false;

            try
            {
#if UNITY_ANDROID
                if (RemoteConfigController.GetBoolConfig("on_review_inapp_rate", false))
                {
                    if (Context.GetSDKLevel() >= MIN_API_LEVEL_REVIEW)
                        reviewInAppController.ShowReview(() => { Application.OpenURL(Config.OPEN_LINK_RATE); });
                    else
                        Application.OpenURL(Config.OPEN_LINK_RATE);
                }
                else
                {
                    Application.OpenURL(Config.OPEN_LINK_RATE);
                }
#else
            Application.OpenURL(Config.OPEN_LINK_RATE);
           // OnCloseWithNeverShow();
#endif
            }
            catch
            {
            }

            CloseAction();
        }
        else
        {
            ShowTextThankRate();
            CloseAction();
        }
    }

    public void CloseAction()
    {
        GameController.Instance.musicManager.PlayClickSound();
        Close();
    }

    public void ShowTextThankRate()
    {
        //StartCoroutine(Helper.StartAction(() =>
        //{
        GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
        (
            btnConfirm.transform.position,
            "Thank you for the review!",
            Color.white,
            true
        );
        // }, 0.5f));
    }
}