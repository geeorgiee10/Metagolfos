using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Linq;
using Helpers.Linq;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
	public static GameState State => Instance._gameState;
	[SerializeField] private GameState _gameState;

	// Session Config
	[Networked, OnChangedRender(nameof(OnSessionConfigChanged))]
	public int CourseLengthIndex { get; set; }
	[Networked, OnChangedRender(nameof(OnSessionConfigChanged))]
	public int MaxTimeIndex { get; set; }
	[Networked, OnChangedRender(nameof(OnSessionConfigChanged))]
	public int MaxStrokesIndex { get; set; }
	[Networked, OnChangedRender(nameof(OnSessionConfigChanged))]
	public bool DoCollisions { get; set; }

	public static int CourseLength => SessionSetup._lengths[Instance.CourseLengthIndex];
	public static float MaxTime => SessionSetup._times[Instance.MaxTimeIndex].v;
	public static int MaxStrokes => SessionSetup._shots[Instance.MaxStrokesIndex].v;

	// Gameplay Data
	[Networked]
	public int CurrentHole { get; set; }
	[Networked]
	public int TickStarted { get; set; }
	public static float Time => Instance?.Object?.IsValid == true
		? (Instance.TickStarted == 0 
			? 0
			: (Instance.Runner.Tick - Instance.TickStarted) * Instance.Runner.DeltaTime)
		: 0;

	public static GameManager Instance { get; private set; }

	void OnSessionConfigChanged()
	{
		InterfaceManager.Instance.sessionScreen.UpdateSessionConfig();
	}

	public override void Spawned()
	{
		Instance = this;
		Runner.AddCallbacks(this);
		if (Runner.IsServer)
		{
			CourseLengthIndex = SessionSetup.courseLength;
			MaxTimeIndex = SessionSetup.maxTime;
			MaxStrokesIndex = SessionSetup.maxShots;
			DoCollisions = SessionSetup.doCollisions;
			if (Runner.SessionInfo.IsVisible != !SessionSetup.isPrivate)
				Runner.SessionInfo.IsVisible = !SessionSetup.isPrivate;
		}
		
		if (State.Current < GameState.EGameState.Loading)
		{
			UIScreen.Focus(InterfaceManager.Instance.sessionScreen.screen);
		}
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		Instance = null;
		InterfaceManager.Instance.resultsScreen.Clear();
	}

	public void OnSceneLoadDone(NetworkRunner runner)
	{
		if (runner.SimulationUnityScene.name == "Game")
			Level.Load(ResourcesManager.Instance.levels[CurrentHole]);
	}

	[Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
	public void Rpc_LoadDone(RpcInfo info = default)
	{
		PlayerRegistry.GetPlayer(info.Source).IsLoaded = true;
	}

	public static void PlayerDNF(PlayerObject player)
	{
		player.TimeTaken = PlayerObject.TIME_DNF;
		player.Strokes = int.MaxValue;
		Instance.Runner.Despawn(player.Controller.Object);

		if (PlayerRegistry.All(p => p.HasFinished))
		{
			State.Server_SetState(GameState.EGameState.Outro);
		}
	}

	public static void CalculateScores()
	{
		// get orderings of fastest times and least strokes
		PlayerObject[] speedOrder = PlayerRegistry.OrderAsc(p => p.TimeTaken, p => !p.IsSpectator).ToArray();
		PlayerObject[] strokeOrder = PlayerRegistry.OrderAsc(p => p.Strokes, p => !p.IsSpectator).ToArray();

		// get ranks for stroke counts -- the same stroke count is the same rank
		byte[] strokeRanks = new byte[PlayerRegistry.CountPlayers];
		int record = -1;
		byte rank = 0;
		for (int i = 0; i < PlayerRegistry.CountPlayers; i++)
		{
			PlayerObject p = strokeOrder[i];
			if (p.Strokes > record)
			{
				record = p.Strokes;
				rank++;
			}
			strokeRanks[i] = rank;
		}
		
		// tally together scores for speed and strokes
		// higher is better: 1st = num players, last = 1
		Dictionary<PlayerObject, byte> scores = new Dictionary<PlayerObject, byte>();
		for (byte i = 0; i < speedOrder.Length; i++)
		{
			byte value = (byte)(PlayerRegistry.CountPlayers - i);
			//Debug.Log(value);
			scores.Add(speedOrder[i], value);
		}
		for (int i = 0; i < strokeRanks.Length; i++)
		{
			byte value = (byte)(PlayerRegistry.CountPlayers - strokeRanks[i] + 1);
			//Debug.Log(value);
			scores[strokeOrder[i]] += value;
		}

		// put the scores to each player's networked scores array
		foreach (KeyValuePair<PlayerObject, byte> kvp in scores)
		{
			kvp.Key.Scores.Set(Instance.CurrentHole, kvp.Value);
		}

		// notify UI to update
		InterfaceManager.Instance.resultsScreen.SetRoundScores();

		if (!PlayerObject.Local.IsSpectator)
		{
			// get the ranks for the local player
			int speedRank = speedOrder.FirstIndex(PlayerObject.Local) + 1;
			int strokeRank = strokeRanks.ElementAt(strokeOrder.FirstIndex(PlayerObject.Local));

			InterfaceManager.Instance.performance.SetTimesText(PlayerObject.Local.TimeTaken, speedRank);
			InterfaceManager.Instance.performance.SetStrokesText(PlayerObject.Local.Strokes, strokeRank);
		}
	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
		if (shutdownReason != ShutdownReason.Ok)
			DisconnectUI.OnShutdown(shutdownReason);
	}

	#region INetworkRunnerCallbacks
	public void OnConnectedToServer(NetworkRunner runner) { }
	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	public void OnDisconnectedFromServer(NetworkRunner runner) { }
	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
	public void OnInput(NetworkRunner runner, NetworkInput input) { }
	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
	public void OnSceneLoadStart(NetworkRunner runner) { }
	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}
