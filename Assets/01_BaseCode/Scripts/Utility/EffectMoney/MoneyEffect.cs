using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MoneyEffect : MonoBehaviour
{
    public enum TypeMoveEffect
    {
        MoveToCome = 0,
        FlyUp = 1
    }

    public TypeMoveEffect typeMoveEffect;
    public CanvasGroup canvasGroup;
    public Image iconItem;
    public Text addValueTxt;
    public Text textContent;
    [SerializeField] private Transform childObj;
    private bool isFollowObject;
    private GameObject objectFollow;
    private Vector3 offsetFollow;

    private void Update()
    {
        if (isFollowObject)
            if (objectFollow != null)
            {
                var targetCamPos = objectFollow.transform.position + offsetFollow;
                // Smoothly interpolate between the camera's current position and it's target position.
                transform.position = targetCamPos;
            }
    }
    //[SerializeField] private GiftDatabase giftDatabase;


    public void SetUpMoveCome(Vector3 posCome, GiftType itemType, int value, UnityAction actionCome,
        bool isFollowObject = false, GameObject objectFollow = null)
    {
        addValueTxt.gameObject.SetActive(false);
        iconItem.gameObject.SetActive(true);
        textContent.gameObject.SetActive(false);

        this.isFollowObject = isFollowObject;
        this.objectFollow = objectFollow;
        if (isFollowObject && objectFollow != null)
            offsetFollow = transform.position - objectFollow.transform.position;

        typeMoveEffect = TypeMoveEffect.MoveToCome;
        childObj.DOKill();
        childObj.localScale = Vector3.zero;
        childObj.transform.localPosition = Vector3.zero;
        childObj.DOScale(1, 0.3f).SetEase(Ease.OutBack).SetUpdate(true).OnComplete(() =>
        {
            posCome = Camera.main.WorldToScreenPoint(posCome);

            childObj.DOMove(posCome, 0.5f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
            {
                gameObject.SetActive(false);
                if (actionCome != null) actionCome();
            });

            SetSpriteIcon(itemType);
            addValueTxt.gameObject.SetActive(false);
        });
    }


    public void SetSpriteIcon(GiftType itemType)
    {
        if (GameController.Instance.dataContain.giftDatabase.GetGift(itemType, out var gift))
        {
            iconItem.sprite = gift.getGiftSprite;
            iconItem.gameObject.SetActive(true);
        }
        else
        {
            iconItem.sprite = null;
            iconItem.gameObject.SetActive(false);
        }
    }

    public void SetMoveFlyUp(GiftType itemType, int value, Color colorText, bool isFollowObject = false,
        GameObject objectFollow = null)
    {
        addValueTxt.gameObject.SetActive(true);
        iconItem.gameObject.SetActive(true);
        textContent.gameObject.SetActive(false);

        this.isFollowObject = isFollowObject;
        this.objectFollow = objectFollow;
        if (isFollowObject && objectFollow != null)
            offsetFollow = transform.position - objectFollow.transform.position;

        typeMoveEffect = TypeMoveEffect.FlyUp;
        SetSpriteIcon(itemType);
        addValueTxt.gameObject.SetActive(true);
        if (value >= 0)
            addValueTxt.text = "+" + value;
        else if (value < 0) addValueTxt.text = "" + value;
        addValueTxt.color = colorText;

        childObj.DOKill();
        childObj.localScale = Vector3.zero;
        childObj.transform.localPosition = Vector3.zero;
        canvasGroup.DOKill();
        canvasGroup.alpha = 1;
        childObj.DOScale(1, 0.3f).SetUpdate(true).SetEase(Ease.OutBack).OnComplete(() =>
        {
            Vector2 posCome = GetPointDistanceFromObject(250, Vector2.up, transform.position);
            childObj.DOMove(posCome, 3).SetUpdate(true).OnComplete(() => { gameObject.SetActive(false); });
            canvasGroup.DOFade(0f, 3f).SetUpdate(true);
            //
        });

        //
    }

    public void SetMoveFlyUpCoinPlayer(GiftType itemType, int value, Color colorText, bool isFollowObject = false,
        GameObject objectFollow = null)
    {
        addValueTxt.gameObject.SetActive(true);
        iconItem.gameObject.SetActive(true);
        textContent.gameObject.SetActive(false);

        this.isFollowObject = isFollowObject;
        this.objectFollow = objectFollow;
        if (isFollowObject && objectFollow != null)
            offsetFollow = transform.position - objectFollow.transform.position;

        typeMoveEffect = TypeMoveEffect.FlyUp;
        SetSpriteIcon(itemType);
        addValueTxt.gameObject.SetActive(true);
        if (value >= 0)
            addValueTxt.text = "+" + value;
        else
            addValueTxt.text = "" + value;
        addValueTxt.color = colorText;

        childObj.DOKill();
        childObj.localScale = Vector3.one;
        childObj.transform.localPosition = Vector3.zero;
        canvasGroup.DOKill();
        canvasGroup.alpha = 1;
        childObj.DOScale(1, 0.1f).SetUpdate(true).SetEase(Ease.OutBack).OnComplete(() =>
        {
            Vector2 posCome = GetPointDistanceFromObject(65, Vector2.up, transform.position);
            childObj.DOMove(posCome, 0.25f).SetUpdate(true).OnComplete(() =>
            {
                canvasGroup.DOFade(0f, 0.1f).SetUpdate(true).OnComplete(() => { gameObject.SetActive(false); });
            });
            // 
            //
        });

        //
    }


    public void SetMoveTextFlyUp(string value, Color colorText, bool isFollowObject = false,
        GameObject objectFollow = null)
    {
        this.isFollowObject = isFollowObject;
        this.objectFollow = objectFollow;
        if (isFollowObject && objectFollow != null)
            offsetFollow = transform.position - objectFollow.transform.position;

        typeMoveEffect = TypeMoveEffect.FlyUp;

        addValueTxt.gameObject.SetActive(false);
        iconItem.gameObject.SetActive(false);

        textContent.gameObject.SetActive(true);
        textContent.text = value;
        textContent.color = colorText;

        childObj.DOKill();
        childObj.localScale = Vector3.zero;
        childObj.transform.localPosition = Vector3.zero;
        canvasGroup.DOKill();
        canvasGroup.alpha = 1;
        childObj.DOScale(1, 0.2f).SetUpdate(true).SetEase(Ease.OutBack).OnComplete(() =>
        {
            Vector2 posCome = GetPointDistanceFromObject(180, Vector2.up, transform.position);
            childObj.DOMove(posCome, 1.5f).SetUpdate(true).OnComplete(() => { gameObject.SetActive(false); });
            canvasGroup.DOFade(0f, 1.2f).SetUpdate(true);
        });

        //
    }

    public void SetMoveTextFlyUpTypePlayer(string value, Color colorText, bool isFollowObject = false,
        GameObject objectFollow = null)
    {
        this.isFollowObject = isFollowObject;
        this.objectFollow = objectFollow;
        if (isFollowObject && objectFollow != null)
            offsetFollow = transform.position - objectFollow.transform.position;

        typeMoveEffect = TypeMoveEffect.FlyUp;
        addValueTxt.color = colorText;
        addValueTxt.gameObject.SetActive(false);
        iconItem.gameObject.SetActive(false);

        textContent.gameObject.SetActive(true);
        textContent.text = value;
        textContent.color = colorText;

        childObj.DOKill();
        childObj.localScale = Vector3.one;
        childObj.transform.localPosition = Vector3.zero;
        canvasGroup.DOKill();
        canvasGroup.alpha = 1;
        childObj.DOScale(1, 0.1f).SetUpdate(true).SetEase(Ease.OutBack).OnComplete(() =>
        {
            Vector2 posCome = GetPointDistanceFromObject(65, Vector2.up, transform.position);
            childObj.DOMove(posCome, 0.45f).SetUpdate(true).OnComplete(() =>
            {
                canvasGroup.DOFade(0f, 0.1f).SetUpdate(true).OnComplete(() => { gameObject.SetActive(false); });
            });
            // 
            //
        });

        //
    }

    public static Vector3 GetPointDistanceFromObject(float distance, Vector3 direction, Vector3 fromPoint)
    {
        distance -= 1;
        //if (distance < 0)
        //    distance = 0;

        var finalDirection = direction + direction.normalized * distance;
        var targetPosition = fromPoint + finalDirection;

        return targetPosition;
    }
}