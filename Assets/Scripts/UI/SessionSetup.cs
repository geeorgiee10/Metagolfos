using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Helpers.Array;

public class SessionSetup : MonoBehaviour
{
	public static string sessionName;
	public static int courseLength;
	public static int maxTime;
	public static int maxShots;
	public static bool doCollisions;
	public static bool isPrivate;

	public TMP_Dropdown[] lengthDropdowns;
	public TMP_Dropdown[] timesDropdowns;
	public TMP_Dropdown[] shotsDropdowns;
	public Toggle[] collisionToggles;

	public UnityEvent onTryCreateSession;
	public UnityEvent onSessionCreated;

	public string SessionName { get => sessionName; set => sessionName = value; }
	
	public bool IsPrivate
	{
		get => isPrivate;
		set
		{
			isPrivate = value;
			if (GameManager.Instance?.Runner != null) ApplyConfig();
		}
	}
	public bool DoCollisions
	{
		set
		{
			doCollisions = value;
			if (GameManager.Instance?.Runner != null) ApplyConfig();
		}
	}

	int _defaultLengthIndex = 3;
	public static readonly int[] _lengths = new int[] { 3, 6, 9, 18 };

	int _defaultTimesIndex = 4;
	public static readonly (string k, float v)[] _times = new (string, float)[]
	{
		( "30s", 30 ), ( "45s", 45 ), ( "1m", 60 ), ( "1m30s", 90 ), ( "2m", 120 ), ( "No Limit", float.PositiveInfinity )
	};

	int _defaultShotsIndex = 5;
	public static readonly (string k, int v)[] _shots = new (string, int)[]
	{
		( "5", 5 ), ( "8", 8 ), ( "10", 10 ), ( "15", 15 ), ( "20", 20 ), ( "No Limit", int.MaxValue )
	};

	public void Init()
	{
		lengthDropdowns.ForEach(d => d.ClearOptions());
		foreach (int l in _lengths)
		{
			lengthDropdowns.ForEach(d => d.options.Add(new TMP_Dropdown.OptionData($"{l}")));
		}
		lengthDropdowns.ForEach(d => d.value = _defaultLengthIndex);
		courseLength = _defaultLengthIndex;

		timesDropdowns.ForEach(d => d.ClearOptions());
		foreach ((string k, float) t in _times)
		{
			timesDropdowns.ForEach(d => d.options.Add(new TMP_Dropdown.OptionData(t.k)));
		}
		timesDropdowns.ForEach(d => d.value = _defaultTimesIndex);
		maxTime = _defaultTimesIndex;

		shotsDropdowns.ForEach(d => d.ClearOptions());
		foreach ((string k, float) t in _shots)
		{
			shotsDropdowns.ForEach(d => d.options.Add(new TMP_Dropdown.OptionData(t.k)));
		}
		shotsDropdowns.ForEach(d => d.value = _defaultShotsIndex);
		maxShots = _defaultShotsIndex;

		doCollisions = true;
		collisionToggles.ForEach(t => t.isOn = doCollisions);
	}

	#region UI Hooks

	public void SetLength(int index)
	{
		courseLength = index;
		if (GameManager.Instance?.Runner != null) ApplyConfig();
	}

	public void SetTime(int index)
	{
		maxTime = index;
		if (GameManager.Instance?.Runner != null) ApplyConfig();
	}

	public void SetShots(int index)
	{
		maxShots = index;
		if (GameManager.Instance?.Runner != null) ApplyConfig();
	}

	public void TryCreateSession()
	{
		if (Matchmaker.Instance.Runner == null)
		{
			onTryCreateSession?.Invoke();
			Matchmaker.Instance.SetRoomCode(sessionName);
			Matchmaker.Instance.SetPrivate(IsPrivate);
			Matchmaker.Instance.TryHostSession(() => onSessionCreated?.Invoke());
		}
		else
		{
			ApplyConfig();
			UIScreen.activeScreen.Back();
		}

	}

	public void ApplyConfig()
	{
		GameManager.Instance.CourseLengthIndex = courseLength;
		GameManager.Instance.MaxStrokesIndex = maxShots;
		GameManager.Instance.MaxTimeIndex = maxTime;
		GameManager.Instance.DoCollisions = doCollisions;
		// BUG? setting IsVisible to the value it is already invalidates SessionInfo
		if (GameManager.Instance.Runner.SessionInfo.IsVisible != !IsPrivate)
			GameManager.Instance.Runner.SessionInfo.IsVisible = !IsPrivate;
	}

	#endregion
}