using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shootCooldown : MonoBehaviour
{
    public IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(2f);
        GetComponent<ThirdPersonMovement>().m_Animator.SetBool("isShooting", false);
        GetComponent<ThirdPersonMovement>().m_Animator.SetBool("isAiming", true);
        yield return new WaitForSeconds(1.5f);
        GetComponent<ThirdPersonMovement>().canShoot = true;
        GetComponent<ThirdPersonMovement>().isAiming = true;
        GetComponent<ThirdPersonMovement>().isInShootingPhase = false;
    }
}
