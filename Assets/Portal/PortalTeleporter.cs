using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PortalTeleporter : MonoBehaviour
{
	public Transform player;
	public Transform reciever;

	private bool playerIsOverlapping = false;

	[SerializeField] private CinemachineFreeLook camera;
	[SerializeField] private CinemachineFreeLook aimCamera;

	[SerializeField] private GameObject water;
	[SerializeField] private GameObject water_snow;
	private bool isInSnow = false;

	// Update is called once per frame
	void Update()
	{
		if (playerIsOverlapping)
		{
			Vector3 portalToPlayer = player.position - transform.position;
			float dotProduct = Vector3.Dot(transform.up, portalToPlayer);

			// If this is true: The player has moved across the portal
			if (dotProduct < 0f)
			{
				// Teleport him!
				float rotationDiff = -Quaternion.Angle(transform.rotation, reciever.rotation);
				rotationDiff += 180;
				player.Rotate(Vector3.up, rotationDiff);

				Vector3 positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * portalToPlayer;
				player.position = reciever.position + positionOffset;

				playerIsOverlapping = false;
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			playerIsOverlapping = true;
			aimCamera.enabled = false;
			camera.enabled = false;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			playerIsOverlapping = false;
			camera.enabled = true;
			aimCamera.enabled = true;
			if (isInSnow)
            {
				isInSnow = false;
				water.gameObject.SetActive(true);
				water_snow.gameObject.SetActive(false);
            }
			else
            {
				isInSnow = true;
				water.gameObject.SetActive(false);
				water_snow.gameObject.SetActive(true);
			}
		}
	}
}
