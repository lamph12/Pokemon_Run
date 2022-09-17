using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CoinPicker : MonoBehaviour
{
    public static CoinPicker coinPicker;
    public Text textcoins;
    public int coins = 500;
    private void Awake()
    {
        coinPicker = this;
    }
    private void Start()
    {
        //textcoins.text = "Coins :" + coins.ToString();3
             textcoins.text = coins.ToString() + " :";
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.transform.tag == "coins")
    //    {         
    //        coins++;          
    //        Destroy(other.gameObject);
    //        //Debug.Log("coins" + coins);
    //        textcoins.text = "Coins :" + coins.ToString();

    //    }
    //}
    private void Update()
    {
        textcoins.text = coins.ToString() + " :";
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "coins")
        {
            
           
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("coins"))
        {
            coins++;
            Destroy(other.gameObject);
            //Debug.Log("coins" + coins);
            textcoins.text = coins.ToString() + " :";
        }
    }
}
