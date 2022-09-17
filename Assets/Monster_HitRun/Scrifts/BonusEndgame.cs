using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BonusEndgame : MonoBehaviour
{
    private int Bonuscoins;
    private int Bosscoins;
    [SerializeField] private Text Bonuscoins_txt;
    [SerializeField] private Text Bosscoins_txt;
    private void Start()
    {
        Bonuscoins = PlayerManager.PlayerManagerIstance.lvPlayer;
        if (Bonuscoins < 0)
            Bonuscoins_txt.text = "x " + 0.ToString();
        else
        Bonuscoins_txt.text = "x " + Bonuscoins.ToString();
        Bosscoins_txt.text = "x " + Bosscoins.ToString();
        StartCoroutine(Settime());
    }

    private IEnumerator Settime()
    {
        yield return new WaitForSeconds(2);
        transform.gameObject.SetActive(false);
    }
    private void Update()
    {
        
    }

}
