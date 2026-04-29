using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class PlayerSessionItemUI : NetworkBehaviour
{
	public TMP_Text usernameText;
	public Image avatar, border;
	public GameObject leaderObj;

	bool isHierarchyChanging = false;

	PlayerObject _player = null;
	PlayerObject Player
	{
		get
		{
			if (_player == null) _player = PlayerRegistry.GetPlayer(Object.InputAuthority);
			return _player;
		}
	}

	private void OnBeforeTransformParentChanged()
	{
		isHierarchyChanging = true;
	}

	private void OnTransformParentChanged()
	{
		isHierarchyChanging = false;
	}

	private void OnDisable()
	{
		if (!isHierarchyChanging && Runner?.IsRunning == true) Runner.Despawn(Object);
	}

	public void Init()
    {
		Player.OnStatChanged += UpdateStats;
		Player.OnSpectatorChanged += SetPosition;
		UpdateStats();
	}

	public override void Spawned()
	{
		Init();

		SetPosition();
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (Player)
		{
			Player.OnStatChanged -= UpdateStats;
			Player.OnSpectatorChanged -= SetPosition;
		}
	}

	void UpdateStats()
	{
		SetUsername(Player.Nickname);
		SetColour(Player.Color);
	}

	void SetPosition()
	{
		if (Player)
		{
			transform.SetParent(
				Player.IsSpectator
				? InterfaceManager.Instance.sessionScreen.spectatorItemHolder
				: InterfaceManager.Instance.sessionScreen.playerItemHolder,
				false);
		}
		else
		{
			transform.SetParent(InterfaceManager.Instance.sessionScreen.playerItemHolder, false);
		}

		bool anySpectators = InterfaceManager.Instance.sessionScreen.spectatorItemHolder.childCount > 0;
		InterfaceManager.Instance.sessionScreen.spectatorHeader.SetActive(anySpectators);
		InterfaceManager.Instance.sessionScreen.spectatorPanel.gameObject.SetActive(anySpectators);
	}

	public void SetUsername(string name)
    {
        usernameText.text = name;
    }

    public void SetColour(Color col)
    {
        usernameText.color = avatar.color = border.color = col;
    }

    public void SetLeader(bool set)
    {
        leaderObj.SetActive(set);
    }
}
