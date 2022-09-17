using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_pool : MonoBehaviour
{
    Transform vitri;
    private Transform spawnPos;
    private Transform targetPos;
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.transform.tag == "enemies")
    //    {
    //        gameObject.SetActive(false);
    //        Destroy(collision.transform.gameObject);
    //    }
    //}
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("enemies"))
        {
            gameObject.SetActive(false);
            Destroy(other.gameObject);

        }


    }
    public void Init(Transform spawnPos, Transform targetPos)
    {
        //Debug.LogError("spawn");
        this.spawnPos = spawnPos;
        this.targetPos = targetPos;
        transform.position = spawnPos.position;
        transform.gameObject.SetActive(true);
    }
    public void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, Vector3.Lerp(transform.position, targetPos.transform.position, 0.7f), 0.7f);
    }
   
    void Update()
    {
        //Debug.LogError("update");

        if (targetPos != null)
        {
            Move();
        }
    }
}
