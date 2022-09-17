using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buy_StartLv : MonoBehaviour
{
    
    private int usecoins;
    private int coinsupStart;
    int Properties=1;

    public Text usecoinstext;
    public Text propertiestext;
    

    
    private void Start()
    {
        usecoins = 50;
        coinsupStart = 10;
        Properties =1;
        //coinsup = PlayerPrefs.GetInt("coinsup");
        //usecoins = PlayerPrefs.GetInt("usecoins");
        //Properties = PlayerPrefs.GetInt("Properties");
        PlayerManager.PlayerManagerIstance.lvPlayer = Properties;

    }
    private void Update()
    {
        
        usecoinstext.text = usecoins.ToString();
        propertiestext.text = "LV "+Properties.ToString();

    }
    public void Buy()
    {
        Debug.Log("usecoins" + usecoins);
        if (CoinPicker.coinPicker.coins >= usecoins)
        {
            
            CoinPicker.coinPicker.coins -= usecoins;
            Properties ++;
            PlayerManager.PlayerManagerIstance.lvPlayer++;
            usecoins += coinsupStart;
            coinsupStart += 10;
            PlayerPrefs.SetInt("coinsup", coinsupStart);
            PlayerPrefs.SetInt("usecoins", usecoins);
            PlayerPrefs.SetInt("Properties", Properties);
        }
    }
}

