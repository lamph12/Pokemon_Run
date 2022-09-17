using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    public static MenuManager MenuManagerIstance;

    public bool GameStace;
    public GameObject menuElement;
    public GameObject YouLose;
    public GameObject Again;
    public GameObject BonusEndgame;
    Ray ray;
    RaycastHit hit;
    void Start()
    {
        MenuManagerIstance = this;
        GameStace = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    
    }
    public void StartGame()
    {
        Debug.Log("start game");
        GameStace = true;
        menuElement.SetActive(false);
    }
    public void EndGame()
    {
        YouLose.SetActive(false);
        YouLose.SetActive(false);
        Again.SetActive(true);
    }
    public void Retry_btn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Active Scene : " + SceneManager.GetActiveScene().name);
    }

   
}
