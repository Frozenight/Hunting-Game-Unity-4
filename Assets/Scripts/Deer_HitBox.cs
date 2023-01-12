using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deer_HitBox : MonoBehaviour
{

    private GameObject obj;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
        {
            obj = GameObject.Find("Deer");
            obj.GetComponent<Deer_controller>().Die();
        }
    }
}
