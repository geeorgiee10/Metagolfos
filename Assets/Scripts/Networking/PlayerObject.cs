using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;

public class PlayerObject : NetworkBehaviour
{
	public const float TIME_UNSET = -1f;
	public const float TIME_DNF = 0xFFFF;
	public const int STROKES_DNF = int.MaxValue;

	public static PlayerObject Local { get; private set; }
	
	// Metadata
	[Networked]
	public PlayerRef Ref { get; set; }
	[Networked]
	public byte Index { get; set; }
	[Networked]
	public Putter Controller { get; set; }

	// User Settings
	[Networked, OnChangedRender(nameof(StatChanged))]
	public string Nickname { get; set; }
	[Networked, OnChangedRender(nameof(StatChanged))]
	public Color Color { get; set; }

	// State & Gameplay Info
	[Networked]
	public bool IsLoaded { get; set; }
	[Networked, OnChangedRender(nameof(SpectatorChanged))]
	public bool IsSpectator { get; set; }
	[Networked]
	public int Strokes { get; set; }
	[Networked, OnChangedRender(nameof(TimeTakenChanged))]
	public float TimeTaken { get; set; } // default set in inspector to -1
	public bool HasFinished => TimeTaken != TIME_UNSET;
	[Networked, Capacity(18)]
	public NetworkArray<byte> Scores { get; }
	public int TotalScore => Scores.Sum(s => s);

	public event System.Action OnStatChanged;
	public event System.Action OnSpectatorChanged;

	public void Server_Init(PlayerRef pRef, byte index)
	{
		Debug.Assert(Runner.IsServer);

		Ref = pRef;
		Index = index;
		Color = Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1);
	}

	public override void Spawned()
	{
		if (Object.HasStateAuthority)
		{
			PlayerRegistry.Server_Add(Runner, Object.InputAuthority, this);
		}

		if (Local) AudioManager.Play("joinedSessionSFX");

		if (Object.HasInputAuthority)
		{
			Local = this;
			Rpc_SetNickname(!string.IsNullOrWhiteSpace(UserData.Nickname) ? UserData.Nickname : $"Golfer{Random.Range(100, 1000)}");
		}

		PlayerRegistry.PlayerJoined(Object.InputAuthority);
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (Local == this) Local = null;

		if (!runner.IsShutdown)
		{
			if (Controller)
			{
				runner.Despawn(Controller.Object);
			}

			if (GameManager.State.Current == GameState.EGameState.Game && PlayerRegistry.All(p => p.Controller == null))
			{
				GameManager.State.Server_SetState(GameState.EGameState.Outro);
			}
		}

	}

	public void ClearGameplayData()
	{
		Strokes = 0;
		TimeTaken = TIME_UNSET;
		for (int i = 0; i < Scores.Length; i++)
		{
			Scores.Set(i, 0);
		}
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	void Rpc_SetNickname(string nick)
	{
		Nickname = nick;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_SetColor(Color color)
	{
		Color = color;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_ToggleSpectate()
	{
		IsSpectator = !IsSpectator;
	}

	void StatChanged()
	{
		OnStatChanged?.Invoke();
	}

	void SpectatorChanged()
	{
		OnSpectatorChanged?.Invoke();
	}

	void TimeTakenChanged()
	{
		if (TimeTaken != TIME_UNSET)
		{
			if (Object.HasInputAuthority)
			{
				HUD.ForceHideAll();
			}
		}
	}
}
