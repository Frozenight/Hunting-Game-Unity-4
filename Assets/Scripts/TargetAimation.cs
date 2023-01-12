using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAimation : MonoBehaviour
{
    public Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
       if (other.tag == "Arrow")
        {
            if (GetComponentInChildren<ArrowTarget>().IsHit)
            {
                anim.SetBool("IsHit", true);
            }
        }
    }
}
