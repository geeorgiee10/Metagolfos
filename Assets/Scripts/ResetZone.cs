using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResetZone : MonoBehaviour
{
	public bool useEffect;

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out Putter player) && GameManager.Instance.Runner.IsServer)
		{
			player.Rpc_Respawn(useEffect);
		}
	}
}