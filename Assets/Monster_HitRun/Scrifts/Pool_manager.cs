using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool_manager : MonoBehaviour
{
    Rigidbody rg;

    [System.Serializable]
    public class Pool
    {
        public string name;
        public GameObject doituong;
        public int size;

    }

    public static Pool_manager Pool_managerInstance;
    public List<Pool> _pool;
    Dictionary<string, Queue<Bullet_pool>> pooldictionary;
    float thoigian = 0f;

    private void Awake()
    {
        Pool_managerInstance = this;
    }
    void Start()
    {
        pooldictionary = new Dictionary<string, Queue<Bullet_pool>>();

        foreach (Pool pool in _pool)
        {
            Queue<Bullet_pool> objectpool = new Queue<Bullet_pool>();

            for (int i = 0; i < pool.size; i++)
            {
                Bullet_pool bullet = Instantiate(pool.doituong).GetComponent<Bullet_pool>();
                bullet.gameObject.SetActive(false);
                objectpool.Enqueue(bullet);
            }
            pooldictionary.Add(pool.name, objectpool);
        }
    }
    //public void spawnpool(string name, Transform posBullet)
    //{

    //    GameObject objspam = pooldictionary[name].Dequeue();
    //    objspam.SetActive(true);
    //    objspam.transform.position = posBullet.position;
    //    rg = objspam.GetComponent<Rigidbody>();
    //    rg.AddForce(posBullet.transform.up * 1000);
    //    pooldictionary[name].Enqueue(objspam);

    //}
    
    public void spawnpool_enemy(string name, Transform posBullet,GameObject hit)
        {
        //Debug.LogError("HIt pos" + hit.transform.position);
        if (Time.time >= thoigian)
        {
            Bullet_pool bullet = pooldictionary[name].Dequeue();
            bullet.Init(posBullet, hit.transform);
            pooldictionary[name].Enqueue(bullet);
            thoigian = Time.time + 0.02f;
        }
    }

}

