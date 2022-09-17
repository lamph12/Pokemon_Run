using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RewardElement : MonoBehaviour
{
    public Transform animParent;
    public Image iconImg;
    [SerializeField] private Text valueTxt;
    [SerializeField] private GameObject rotateObj;
    public Transform aura;
    private int value;

    private void Update()
    {
        aura.localEulerAngles += new Vector3(0, 0, 30) * Time.deltaTime;
    }

    public void Init(Sprite iconSpr, int value, GameObject rewardAnim = null, bool isAnim = true)
    {
        foreach (Transform child in animParent) SimplePool.Despawn(child.gameObject);
        iconImg.gameObject.SetActive(false);

        this.value = value;

        if (iconImg != null && iconSpr != null)
        {
            iconImg.gameObject.SetActive(true);
            iconImg.sprite = iconSpr;
        }

        if (valueTxt != null)
        {
            valueTxt.gameObject.SetActive(true);
            if (value == 0)
                valueTxt.text = "";
            else
                valueTxt.text = value.ToString();
        }

        if (rewardAnim != null)
        {
            var rw = SimplePool.Spawn(rewardAnim);
            rw.transform.SetParent(animParent);
            rw.transform.localScale = Vector3.one;
            rw.transform.position = Vector3.zero;
            rw.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }

        if (isAnim)
        {
            transform.localScale = Vector3.zero;
            transform.DOKill();
            transform.DOScale(1, 0.3f).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (rotateObj != null) rotateObj.gameObject.SetActive(true);
            });
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }


    public void Init(int playerId)
    {
        iconImg.gameObject.SetActive(false);
        valueTxt.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void MakeX2()
    {
        StartCoroutine(DoX2());
    }

    private IEnumerator DoX2()
    {
        var curVal = value;
        var tarVal = 2 * value;

        var delta = (tarVal - curVal) / 10;

        if (delta == 0) delta = 1;

        while (curVal < tarVal)
        {
            curVal += delta;
            if (curVal > tarVal) curVal = tarVal;

            DOTween.Sequence().Append(valueTxt.gameObject.GetComponent<RectTransform>().DOScale(1.1f, 0.01f))
                .Append(valueTxt.gameObject.GetComponent<RectTransform>().DOScale(1f, 0.01f));
            valueTxt.text = curVal.ToString();

            yield return new WaitForSeconds(0.02f);
        }
    }
}