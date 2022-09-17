using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOff : MonoBehaviour
{
    [SerializeField] private GameObject onimage;
    [SerializeField]private GameObject offimage;
    [SerializeField]private GameObject on;
    [SerializeField]private GameObject off;
    [SerializeField]private bool muted=false;
    public void Start()
    {
        if (!PlayerPrefs.HasKey("muted"))
        {
            PlayerPrefs.SetInt("muted", 1);
            Load();

        }
        else
        {
            Load();
        }

    }
    public void OnObject()
    {
        if (muted==false)
        {
            muted = true;
            onimage.SetActive(true);
            offimage.SetActive(false);
            on.SetActive(true);
            off.SetActive(false);
        }
        else
        {
            muted = false;
            Debug.Log("da vao off");
            onimage.SetActive(false);
            offimage.SetActive(true);
            on.SetActive(false);
            off.SetActive(true);
        }
        Save();
    }
    
    private void Load()
    {
        muted = PlayerPrefs.GetInt("muted") == 1;
    }
    private void Save()
    {
        PlayerPrefs.SetInt("muted", muted ? 1 : 0);
    }

    

}
