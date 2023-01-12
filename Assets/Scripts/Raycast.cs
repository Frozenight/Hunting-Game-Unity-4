using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.down) * 0.1f;
        Debug.DrawRay(transform.position, forward, Color.green);
    }
}
