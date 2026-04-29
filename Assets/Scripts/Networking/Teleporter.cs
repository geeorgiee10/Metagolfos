using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Teleporter : MonoBehaviour
{
	public Transform destination;

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out Rigidbody rb))
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.position = destination.position;
		}
	}
}