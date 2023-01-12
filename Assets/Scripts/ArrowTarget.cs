using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTarget : MonoBehaviour
{
    private int hitCounter = 0;
    private int targetCounter = 1;

    public bool IsHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
            hitCounter++;
        if (hitCounter >= targetCounter)
        {
            IsHit = true;
            StartCoroutine(Cooldown());
        }
    }

    public IEnumerator Cooldown()
    {
        GetComponentInParent<TargetAimation>().anim.SetBool("IsReady", false);
        yield return new WaitForSeconds(3f);
        IsHit = false;
        GetComponentInParent<TargetAimation>().anim.SetBool("IsReady", true);
        GetComponentInParent<TargetAimation>().anim.SetBool("IsHit", false);
    }
}
