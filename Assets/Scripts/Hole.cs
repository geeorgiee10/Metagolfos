using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
	private void OnTriggerStay(Collider other)
	{
		if (GameManager.Instance.Runner.IsServer)
		{
			if (other.TryGetComponent(out Putter player) && player.rb.velocity.magnitude <= 0.5f)
			{
				player.PlayerObj.TimeTaken = (GameManager.Instance.Runner.Tick - GameManager.Instance.TickStarted) * GameManager.Instance.Runner.DeltaTime;
				GameManager.Instance.Runner.Despawn(player.Object);

				if (PlayerRegistry.All(p => p.HasFinished))
				{
					GameManager.State.Server_SetState(GameState.EGameState.Outro);
				}
			}
		}
	}
}
