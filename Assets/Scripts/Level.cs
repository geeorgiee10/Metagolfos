using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Level : NetworkBehaviour
{
    public static Level Current { get; private set; }

	public float spawnHeight = 1f;

	public static void Load()
	{
		Unload();

		if (!GameManager.Instance.Runner.CanSpawn)
			return;

		var variants = ResourcesManager.Instance
			.levels[GameManager.Instance.CurrentHole]
			.variants;

		int index = GameManager.Instance.SelectedVariant;

		GameManager.Instance.Runner.Spawn(variants[index]);
	}

	public static void Unload()
	{
		if (Current)
		{
			GameManager.Instance.Runner.Despawn(Current.Object);
			Current = null;
		}
	}

	public override void Spawned()
	{
		Current = this;
		GameManager.Instance.Rpc_LoadDone();
	}

	public Vector3 GetSpawnPosition(int index)
	{
		Vector2 p = Random.insideUnitCircle * 0.15f;
		return new Vector3(p.x, spawnHeight, p.y);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(Vector3.up * spawnHeight, 0.03f);
	}
}
