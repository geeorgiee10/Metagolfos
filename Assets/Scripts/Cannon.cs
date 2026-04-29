using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class Cannon : NetworkBehaviour
{
	public float strength = 100;
	public float interval = 3;
	public float offset = 0;

	public Transform checkPos;
	public float checkRadius;

	public override void FixedUpdateNetwork()
	{
		if ((GameManager.Time + offset) % interval <= Runner.DeltaTime)
		{
			foreach (Putter p in Physics.OverlapSphere(checkPos.position, checkRadius)
				.Select(c => c.GetComponent<Putter>())
				.Where(p => p != null))
			{
				p.rb.AddForce(checkPos.forward * strength, ForceMode.VelocityChange);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (checkPos)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(checkPos.position, checkRadius);
		}
	}
}