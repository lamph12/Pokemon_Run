using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buy_Boss : MonoBehaviour
{

    [SerializeField] private int usecoinsBoss;
    private int coinsupBoss;
    private float PropertiesBoss;
    [SerializeField] private Text usecoinstext;
    [SerializeField] private Text propertiestext;
 

    
    private void Start()
    {
        usecoinsBoss = 50;
        PropertiesBoss = 0.5f;
        coinsupBoss = 25;
        usecoinsBoss = 50;
        //coinsupBoss = PlayerPrefs.GetInt("coinsup");
        //usecoinsBoss = PlayerPrefs.GetInt("usecoinsBoss");
        //PropertiesBoss = PlayerPrefs.GetInt("PropertiesBoss");

    }
    private void Update()
    {
        
        usecoinstext.text = usecoinsBoss.ToString();
        propertiestext.text = PropertiesBoss.ToString()+ "%";

    }
    public void Buy()
    {
        Debug.Log("usebuyboss"+usecoinsBoss);
        if (CoinPicker.coinPicker.coins >= usecoinsBoss)
        {
            CoinPicker.coinPicker.coins -= usecoinsBoss;
            PropertiesBoss += 0.5f; ;
            coinsupBoss = 25;
            usecoinsBoss += coinsupBoss;           
            PlayerPrefs.SetInt("coinsup", coinsupBoss);
            PlayerPrefs.SetInt("usecoinsBoss", usecoinsBoss);
            PlayerPrefs.SetFloat("PropertiesBoss", PropertiesBoss);
        }
    }
}

