using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class GameState : NetworkBehaviour
{
	public enum EGameState { Off, Pregame, Loading, Intro, Game, Outro, Postgame }

	[Networked][field: ReadOnly] public EGameState Previous { get; set; }
	[Networked] [field: ReadOnly] public EGameState Current { get; set; }

	[Networked] TickTimer Delay { get; set; }
	[Networked] EGameState DelayedState { get; set; }

	protected StateMachine<EGameState> StateMachine = new StateMachine<EGameState>();

	public override void Spawned()
	{
		if (Runner.IsServer)
		{
			Server_SetState(EGameState.Pregame);
		}

		StateMachine[EGameState.Pregame].onEnter = prev =>
		{
			if (prev == EGameState.Postgame)
			{
				if (Runner.IsServer)
				{
					Runner.LoadScene("Menu");
					if (!Runner.SessionInfo.IsOpen) Runner.SessionInfo.IsOpen = true;
				}
				UIScreen.activeScreen.BackTo(InterfaceManager.Instance.sessionScreen.screen);
			}

			//if (Runner.IsServer)
			//{
			//	//GameManager.Instance.CourseLength = SessionSetup.courseLength;
			//	//GameManager.Instance.MaxTime = SessionSetup.maxTime;
			//	//GameManager.Instance.MaxShots = SessionSetup.maxShots;
			//	//GameManager.Instance.DoCollisions =
			//	//GameManager.Instance.DoCollisions = SessionSetup.doCollisions;
			//}

			
		};

		StateMachine[EGameState.Pregame].onExit = next =>
		{
			if (Runner.SessionInfo.IsOpen) Runner.SessionInfo.IsOpen = false;
		};

		StateMachine[EGameState.Loading].onEnter = prev =>
		{
			int layer = LayerMask.NameToLayer("Ball");
			Physics.IgnoreLayerCollision(layer, layer, !GameManager.Instance.DoCollisions);

			PlayerRegistry.ForEach(p =>
			{
				p.Strokes = 0;
				p.TimeTaken = PlayerObject.TIME_UNSET;
			});

			if (prev == EGameState.Pregame)
			{
				InterfaceManager.Instance.resultsScreen.Init();
				if (Runner.IsServer) Runner.LoadScene("Game");
			}
			else
			{
				GameManager.Instance.CurrentHole++;
				if (Runner.IsServer) Level.Load(ResourcesManager.Instance.levels[GameManager.Instance.CurrentHole]);
			}

			UIScreen.Focus(InterfaceManager.Instance.hud);
		};

		StateMachine[EGameState.Loading].onUpdate = () =>
		{
			if (Runner.IsServer)
			{
				if (PlayerRegistry.All(p => p.IsLoaded, true))
				{
					Server_SetState(EGameState.Intro);
				}
			}
		};

		StateMachine[EGameState.Loading].onExit = next =>
		{
			if (Runner.IsServer)
			{
				PlayerRegistry.ForEach(p => p.IsLoaded = false, true);
				PlayerRegistry.ForEach((p, i) =>
				{
					Runner.Spawn(ResourcesManager.Instance.playerControllerPrefab,
						position: Level.Current.GetSpawnPosition(i),
						inputAuthority: p.Ref);
				});
			}
		};

		StateMachine[EGameState.Intro].onEnter = prev =>
		{
			if (Runner.IsServer)
			{
				PlayerRegistry.ForEach(p =>
				{
					p.Controller.PuttTimer = TickTimer.CreateFromSeconds(Runner, 3);
				});
			}
			CameraController.Recenter();
			HUD.SetLevelName(GameManager.Instance.CurrentHole);
			HUD.SetStrokeCount(0);
			HUD.SetTimerText(0);
			InterfaceManager.Instance.StartCountdown();
			Server_DelaySetState(EGameState.Game, 3);
		};

		StateMachine[EGameState.Game].onEnter = prev =>
		{
			GameManager.Instance.TickStarted = Runner.Tick;
		};

		StateMachine[EGameState.Game].onUpdate = () =>
		{
			HUD.SetTimerText(GameManager.Time);
			if (Runner.IsServer && GameManager.Time >= GameManager.MaxTime)
			{
				Debug.Log("Time's up");
				PlayerRegistry.ForEachWhere(p => p.Controller, p => GameManager.PlayerDNF(p));
			}
		};

		StateMachine[EGameState.Outro].onEnter = prev =>
		{
			GameManager.CalculateScores();
			UIScreen.activeScreen.BackTo(InterfaceManager.Instance.hud);
			UIScreen.Focus(InterfaceManager.Instance.scoreboard);
			UIScreen.Focus(InterfaceManager.Instance.performance.screen);

			GameManager.Instance.TickStarted = 0;

			// if there are more holes to play
			if (GameManager.Instance.CurrentHole + 1 < Mathf.Min(ResourcesManager.Instance.levels.Length, GameManager.CourseLength))
			{
				// then load the next hole and delay set state to intro
				Server_DelaySetState(EGameState.Loading, 5);
			}
			else
			{
				// else go to postgame
				Server_DelaySetState(EGameState.Postgame, 5);
			}
		};

		StateMachine[EGameState.Outro].onExit = next =>
		{
			UIScreen.activeScreen.Back();
		};

		StateMachine[EGameState.Postgame].onEnter = prev =>
		{
			Level.Unload();
			InterfaceManager.Instance.postgameUI.SetWinner(PlayerRegistry.OrderDesc(p => p.TotalScore).First());
			UIScreen.Focus(InterfaceManager.Instance.postgameUI.screen);
			Server_DelaySetState(EGameState.Pregame, 5);
		};

		StateMachine[EGameState.Postgame].onUpdate = () =>
		{
			if (Delay.RemainingTime(Runner) is float t)
			{
				InterfaceManager.Instance.postgameUI.UpdateReturningText(Mathf.CeilToInt(t));
			}
		};

		StateMachine[EGameState.Postgame].onExit = next =>
		{
			InterfaceManager.Instance.resultsScreen.Clear();
			PlayerRegistry.ForEach(p => p.ClearGameplayData());
			GameManager.Instance.CurrentHole = 0;
			GameManager.Instance.TickStarted = 0;
		};

		// Ensures that FixedUpdateNetwork is called for all proxies.
		Runner.SetIsSimulated(Object, true);

		StateMachine.Update(Current, Previous);
	}

	public override void FixedUpdateNetwork()
	{
		if (Runner.IsServer)
		{
			if (Delay.Expired(Runner))
			{
				Delay = TickTimer.None;
				Server_SetState(DelayedState);
			}
		}

		if (Runner.IsForward)
			StateMachine.Update(Current, Previous);
	}

	public void Server_SetState(EGameState st)
	{
		if (Current == st) return;
		//Debug.Log($"Set State to {st}");
		Previous = Current;
		Current = st;
	}
	
	public void Server_DelaySetState(EGameState newState, float delay)
	{
		Debug.Log($"Delay state change to {newState} for {delay}s");
		Delay = TickTimer.CreateFromSeconds(Runner, delay);
		DelayedState = newState;
	}
}