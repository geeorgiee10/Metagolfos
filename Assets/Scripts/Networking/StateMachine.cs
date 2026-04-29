using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : Enum
{
	public class StateHooks
	{
		public Action<T> onEnter;
		public Action<T> onExit;
		public Action onUpdate;
	}

	private Dictionary<T, StateHooks> _states = new Dictionary<T, StateHooks>();
	private (T current, T previous) previousFrame = (default(T), default(T));

	public StateHooks this[T statename]
	{
		get
		{
			if (!_states.TryGetValue(statename, out StateHooks state))
			{
				state = new StateHooks();
				_states[statename] = state;
			}
			return state;
		}
	}

	public void Update(T currentState, T previousState)
	{
		// run state 'stay' logic
		if (_states.TryGetValue(previousFrame.current, out StateHooks current))
			current.onUpdate?.Invoke();

		// if state hasn't changed
		if (Equals(currentState, previousFrame.current) && Equals(previousState, previousFrame.previous))
			return;

		// exit state if we've changed
		if (_states.TryGetValue(previousState, out StateHooks previous))
			previous.onExit?.Invoke(currentState);

		// enter new state
		if (_states.TryGetValue(currentState, out StateHooks next))
			next.onEnter?.Invoke(previousState);

		previousFrame = (currentState, previousState);
	}
}