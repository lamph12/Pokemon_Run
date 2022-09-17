using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseBox : MonoBehaviour
{
    [SerializeField] protected RectTransform mainPanel;

    [HideIf("isPopup", false)] public RectTransform contentPanel;

    [Header("========= CONFIG BOX ===========")]
    public bool isNotStack;

    public bool isPopup;
    [SerializeField] protected bool isAnim = true;
    [HideInInspector] public bool isBoxSave;
    protected UnityAction actionOpenSaveBox;
    protected CanvasGroup canvasGroupPanel;

    protected string iDPopup;

    //Call Back
    public UnityAction OnCloseBox;
    public UnityAction<int> OnChangeLayer;
    protected Canvas popupCanvas;


    private void Awake()
    {
        popupCanvas = GetComponent<Canvas>();
        if (popupCanvas != null && isPopup)
        {
            popupCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            popupCanvas.worldCamera = Camera.main;
            popupCanvas.sortingLayerID = SortingLayer.NameToID("Popup");
        }

        if (mainPanel != null)
        {
            var tweenAnimation = mainPanel.GetComponent<DOTweenAnimation>();
            if (tweenAnimation != null)
            {
                tweenAnimation.isIndependentUpdate = true; //Không phục thuộc vào time scale
                isAnim = false;
            }
        }

        OnAwake();
    }

    protected string GetIDPopup()
    {
        return iDPopup;
    }

    protected virtual void SetIDPopup()
    {
        iDPopup = GetType().ToString();
    }

    public virtual T SetupBase<T>(bool isSaveBox = false, UnityAction actionOpenBoxSave = null) where T : BaseBox
    {
        InitBoxSave(isSaveBox, actionOpenBoxSave);
        return null;
    }

    protected virtual void OnAwake()
    {
    }

    public void InitBoxSave(bool isBoxSave, UnityAction actionOpenSaveBox)
    {
        this.isBoxSave = isBoxSave;
        this.actionOpenSaveBox = actionOpenSaveBox;
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    ///     Đưa popup vào trong Stack save
    /// </summary>
    public virtual void SaveBox()
    {
        if (isBoxSave)
            BoxController.Instance.AddBoxSave(GetIDPopup(), actionOpenSaveBox);
    }

    /// <summary>
    ///     Chủ động gọi Remove Save Box theo trường hợp cụ thể
    /// </summary>
    public virtual void RemoveSaveBox()
    {
        BoxController.Instance.RemoveBoxSave(GetIDPopup());
    }

    #region Change layer Box

    public void ChangeLayerHandle(ref int indexInStack)
    {
        if (isPopup)
        {
            if (popupCanvas != null)
            {
                popupCanvas.sortingOrder = BoxController.BASE_INDEX_LAYER + indexInStack;
                popupCanvas.planeDistance = 5;

                if (OnChangeLayer != null)
                    OnChangeLayer(popupCanvas.sortingLayerID);

                indexInStack += 40;
            }
        }
        else
        {
            if (contentPanel != null)
                transform.SetAsLastSibling();
        }
    }

    #endregion

    public virtual bool IsActive()
    {
        return true;
    }

    #region Init Open Handle

    protected virtual void OnEnable()
    {
        if (!isNotStack)
        {
            if (!isPopup)
                if (canvasGroupPanel != null)
                    canvasGroupPanel.blocksRaycasts = false;

            BoxController.Instance.AddNewBackObj(this);
        }

        SetIDPopup();
        DoAppear();
        OnStart();
    }

    protected virtual void DoAppear()
    {
        if (isAnim)
            if (mainPanel != null)
            {
                mainPanel.localScale = Vector3.zero;
                mainPanel.DOScale(1, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
            }
    }

    protected virtual void OnStart()
    {
    }

    #endregion

    #region Close Box

    public virtual void Close()
    {
        if (!isNotStack)
            BoxController.Instance.Remove();
        DoClose();
    }

    protected virtual void DoClose()
    {
        if (isAnim)
        {
            if (mainPanel != null)
            {
                mainPanel.localScale = Vector3.one;
                mainPanel.DOScale(0, 0.5f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (!isPopup)
            if (canvasGroupPanel != null)
                canvasGroupPanel.blocksRaycasts = true;
    }

    protected virtual void OnDisable()
    {
        if (OnCloseBox != null)
        {
            OnCloseBox();
            OnCloseBox = null;
        }

        if (BoxController.Instance.actionOnClosedOneBox != null)
            BoxController.Instance.actionOnClosedOneBox();
    }

    protected void DestroyBox()
    {
        if (OnCloseBox != null)
            OnCloseBox();
        Destroy(gameObject);
    }

    #endregion
}