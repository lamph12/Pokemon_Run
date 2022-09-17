using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class MiniGame: MonoBehaviour
{
    //private Transform transform;

    public GameObject x2_img;
    public GameObject x3_img;
    public GameObject x5_img;
    public Text xCoins_txt;

    void Start()
    {
        transform.DOLocalMoveX(-227, 1.2f,false).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        
    }

    // Update is called once per frame
    void Update()
    {

        if ((transform.localPosition.x > 105 && transform.localPosition.x <=227)|| (transform.localPosition.x > -227 && transform.localPosition.x <= -105))
        {
            Debug.Log("x222");
            x2_img.SetActive(true);
            x3_img.SetActive(false);
            x5_img.SetActive(false);
            xCoins_txt.text = "x2";
        }
        if((transform.localPosition.x > 29 && transform.localPosition.x <= 105)|| (transform.localPosition.x > -105 && transform.localPosition.x < -29))
        {
            Debug.Log("x33");
            x2_img.SetActive(false);
            x3_img.SetActive(true);
            x5_img.SetActive(false);
            xCoins_txt.text = "x3";

        }
        if (transform.localPosition.x >= -29 && transform.localPosition.x<= 29)
        {
            Debug.Log("x55");
            x2_img.SetActive(false);
            x3_img.SetActive(false);
            x5_img.SetActive(true);
            xCoins_txt.text = "x5";

        }



    }
    public void KillTheAnimatioN()
    {
        transform.DOKill();
    }
}
