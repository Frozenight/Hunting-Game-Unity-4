using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Go_Portal : MonoBehaviour
{
    [SerializeField] private CinemachineBrain camera;

    private bool isLocked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
        }
    }
}
