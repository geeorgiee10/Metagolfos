using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionItemUI : MonoBehaviour
{
	[SerializeField] private TMP_Text sessionNameLabel;
	[SerializeField] private TMP_Text playerCount;
	[SerializeField] private TMP_Text statusText;

	string _sessionName = null;

	public void Init(string sessionName, int players, int maxPlayers, bool isOpen)
	{
		gameObject.SetActive(true);
		sessionNameLabel.text = _sessionName = sessionName;
		playerCount.text = $"{players}/{maxPlayers}";
		statusText.text = isOpen ? "Open" : "Closed";
	}

	public void Disable()
	{
		sessionNameLabel.text =
		playerCount.text =
		_sessionName = null;
		gameObject.SetActive(false);
	}

	public void Join()
	{
		Matchmaker.Instance.TryJoinSession(_sessionName);
	}
}
