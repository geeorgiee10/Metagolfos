using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoostTrigger : MonoBehaviour
{
	public float strength = 10f;
	private void OnTriggerStay(Collider other)
	{
		if (other.TryGetComponent(out Putter player))
		{
			player.rb.AddForce(transform.forward * strength, ForceMode.Force);
		}
	}
}
