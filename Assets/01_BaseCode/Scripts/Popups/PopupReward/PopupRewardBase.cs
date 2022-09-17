using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupRewardBase : BaseBox
{
    private static PopupRewardBase instance;

    [SerializeField] private RewardElement rewardPrefab;
    [SerializeField] private Transform contentPool;

    [SerializeField] private Button claimBtn;

    [SerializeField] private ParticleSystemRenderer[] parslayer;
    [SerializeField] private Canvas canvasRewardContent;

    [SerializeField] private AudioClip rewardClip;
    private Action _actionClaim;

    private readonly List<RewardElement> _poolReward = new List<RewardElement>();
    private List<GiftRewardShow> _reward;

    [HideInInspector] public UnityAction actionMoreClaim;
    private bool isAddValueItem;

    private bool isClickedClaim;
    private bool isClosing;
    private bool isX2Reward;
    private IEnumerator showBtnClaimIE;

    public static PopupRewardBase Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null) instance = Instantiate(Resources.Load<PopupRewardBase>(PathPrefabs.POPUP_REWARD_BASE));
        //instance.Show();
        return instance;
    }

    public void Init()
    {
    }

    public PopupRewardBase Show(List<GiftRewardShow> reward, Action actionClaim = null, float timeShowClaimNow = 0)
    {
        Debug.Log("============= AAA ============== ");
        claimBtn.onClick.RemoveAllListeners();
        claimBtn.onClick.AddListener(Claim);
        claimBtn.interactable = true;
        if (isAnim)
            if (mainPanel != null)
            {
                mainPanel.localScale = Vector3.zero;
                mainPanel.DOScale(1, 0.5f).SetUpdate(true).SetEase(Ease.OutBack).OnComplete(() => { });
            }

        base.Show();
        _actionClaim = actionClaim;
        ClearPool();
        // BoxController.Instance.isLockEscape = true;
        canvasRewardContent.sortingLayerID = SortingLayer.NameToID("Popup");
        canvasRewardContent.sortingOrder = popupCanvas.sortingLayerID + 2;

        for (var i = 0; i < reward.Count; i++)
        {
            var elem = GetRewardElement();

            if (reward[i].icon == null)
            {
                if (reward[i].rewardAnim == null)
                    elem.Init(GetIcon(reward[i].type), reward[i].amount, GetAnim(reward[i].type));
                else
                    elem.Init(GetIcon(reward[i].type), reward[i].amount, reward[i].rewardAnim);
            }
            else

            {
                if (reward[i].rewardAnim != null)
                    elem.Init(reward[i].icon, reward[i].amount, reward[i].rewardAnim);
                else
                    elem.Init(reward[i].icon, reward[i].amount, GetAnim(reward[i].type));
            }

            if (GiftDatabase.IsCharacter(reward[i].type))
                elem.iconImg.transform.localScale = 4f * Vector3.one;
            else
                elem.iconImg.transform.localScale = 1.5f * Vector3.one;
        }

        _reward = reward;

        isClickedClaim = false;
        isClosing = false;

        OnCloseBox = () =>
        {
            isClosing = true;
            Claim();
            if (actionMoreClaim != null)
            {
                actionMoreClaim();
                actionMoreClaim = null;
            }
        };

        claimBtn.gameObject.SetActive(true);
        claimBtn.transform.localScale = Vector3.one;
        isX2Reward = false;

        for (var i = 0; i < parslayer.Length; i++) parslayer[i].sortingOrder = popupCanvas.sortingOrder + 1;

        GameController.Instance.musicManager.PlayOneShot(rewardClip);
        return this;
    }

    private RewardElement GetRewardElement()
    {
        for (var i = 0; i < _poolReward.Count; i++)
            if (!_poolReward[i].gameObject.activeSelf)
            {
                _poolReward[i].gameObject.SetActive(true);
                return _poolReward[i];
            }

        var element = Instantiate(rewardPrefab, contentPool);
        _poolReward.Add(element);
        return element;
    }

    private void ClearPool()
    {
        foreach (var rewardElement in _poolReward) rewardElement.gameObject.SetActive(false);
    }

    public void Claim()
    {
        if (isClickedClaim)
            return;

        ClaimSuccess();
        GameController.Instance.musicManager.PlayClickSound();
    }

    private IEnumerator ClaimWithDelay()
    {
        yield return new WaitForSeconds(0.8f);

        ClaimSuccess();
    }

    public void ClaimSuccess()
    {
        isClickedClaim = true;

        //  BoxController.Instance.isLockEscape = false;
        if (!isClosing)
            Close();

        if (_actionClaim != null)
            _actionClaim();
        claimBtn.interactable = false;
    }

    private Sprite GetIcon(GiftType type)
    {
        return GameController.Instance.dataContain.giftDatabase.GetIconItem(type);
    }

    private GameObject GetAnim(GiftType type)
    {
        return GameController.Instance.dataContain.giftDatabase.GetAnimItem(type);
    }
}

[Serializable]
public class GiftRewardShow
{
    public GiftType type;
    public int amount;
    public Sprite icon;
    public GameObject rewardAnim;
}