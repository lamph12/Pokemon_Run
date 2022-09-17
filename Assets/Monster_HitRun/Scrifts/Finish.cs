using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    public GameObject left;
    public GameObject right;
    public GameObject flagleft;
    public GameObject flagright;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            left.gameObject.SetActive(true);
            right.gameObject.SetActive(true);
            flagleft.transform.rotation = Quaternion.Slerp(flagleft.transform.rotation, Quaternion.Euler(0, 180, 0), 10 );
            flagright.transform.rotation = Quaternion.Slerp(flagright.transform.rotation, Quaternion.Euler(0,0, 0), 10);
            other.gameObject.transform.position = new Vector3(0, other.transform.position.y, other.transform.position.z);
            PlayerManager.PlayerManagerIstance.thewall = true;

        }
    }
}
