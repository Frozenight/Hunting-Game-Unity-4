using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private Rigidbody arrowRigidBody;
    private Collider target;

    private float arrowSpeed = 100f;

    [SerializeField]
    private GameObject vfx;

    private void Awake()
    {
        arrowRigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        float speed = arrowSpeed;
        arrowRigidBody.velocity = transform.forward * speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        arrowRigidBody.velocity = transform.forward * 0f;
        if (other.GetComponent<ArrowTarget>() != null)
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y, transform.position.z - 2);
            Instantiate(vfx, pos, Quaternion.identity);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(destoryArrowAfterSomeTIme());
        }
    }

    public IEnumerator destoryArrowAfterSomeTIme()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
