using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PostGameUI : MonoBehaviour
{
	public UIScreen screen;
	public TMP_Text returningText;
	public TMP_Text winnerText;

	public void SetWinner(PlayerObject player)
	{
		winnerText.text = $"{player.Nickname} is the winner!";
		InterfaceManager.Instance.sessionScreen.golfBallRend.material.color = player.Color;
	}

	public void UpdateReturningText(int time)
    {
		returningText.text = $"Returning to Room in {time}...";
    }
}
