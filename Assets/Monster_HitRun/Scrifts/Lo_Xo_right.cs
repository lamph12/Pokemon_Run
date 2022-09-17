using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lo_Xo_right : MonoBehaviour
{
    Rigidbody rg;
    private void OnTriggerEnter(Collider other)
    {
        rg = other.GetComponent<Rigidbody>();
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("on the wall");
        other.gameObject.transform.Rotate(0, 0, -25);
        rg.velocity= new Vector3(-2.5f, 3, 0);
        rg.isKinematic = false;

    }
}
