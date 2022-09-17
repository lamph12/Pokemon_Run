using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    public int lvenemies;
    public bool shotted;
    private Rigidbody rgEnemies;
    private Collider collider;
    private void Start()
    {
        shotted = false;
        rgEnemies = GetComponent<Rigidbody>();
        collider = transform.gameObject.GetComponent<Collider>();


    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Ball")
        {
            if (PlayerManager.PlayerManagerIstance.lvPlayer >= lvenemies)
                PlayerManager.PlayerManagerIstance.lvPlayer = PlayerManager.PlayerManagerIstance.lvPlayer + lvenemies;
            else
                PlayerManager.PlayerManagerIstance.lvPlayer -= 10;
            shotted = true;
        }
        if (other.gameObject.tag == "Player")
        {
            if (PlayerManager.PlayerManagerIstance.lvPlayer < lvenemies)
            {
                other.gameObject.SetActive(false);
                MenuManager.MenuManagerIstance.GameStace = false;
                MenuManager.MenuManagerIstance.BonusEndgame.gameObject.SetActive(true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            Debug.Log("va cham cung Player");
            adfPlayer();


        }
    }
    public void adfPlayer()
    {
        rgEnemies.AddForce(Vector3.forward*2f);
        collider.isTrigger = true;
    }
}
