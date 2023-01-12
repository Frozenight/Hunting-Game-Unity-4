using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LookAtAnimationRig : MonoBehaviour
{
    private Rig rig;
    private float targetWeight;

    private void Awake()
    {
        rig = GetComponent<Rig>();
    }

    private void Update()
    {
        rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * 10f);
    }

    public void enableAiming()
    {
        targetWeight = 1f;
    }
    public void disableAiming()
    {
        targetWeight = 0f;
    }
}
