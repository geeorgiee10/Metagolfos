using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Helpers.Linq;

public class ResultsScreenUI : MonoBehaviour
{
	public Transform scoreHolder;
	readonly Dictionary<PlayerRef, PlayerScoreboardUI> playerScoreItems = new Dictionary<PlayerRef, PlayerScoreboardUI>();
	public GameObject[] holes;
	
	public void Init()
	{
		InitScoreboard();
		InitPlayerScores();
		PlayerRegistry.OnPlayerLeft -= PlayerLeft;
		PlayerRegistry.OnPlayerLeft += PlayerLeft;
	}

	void InitScoreboard()
    {
		SetHoleCount(GameManager.CourseLength);
    }

	void InitPlayerScores()
	{
		if (GameManager.Instance.Runner.IsServer)
		{
			PlayerRegistry.ForEach(p =>
			{
				PlayerScoreboardUI playerScore = GameManager.Instance.Runner.Spawn(ResourcesManager.Instance.playerScoreUI);
				playerScore.Init(p);
			});
		}
	}

	public void SetHoleCount(int count)
    {
        foreach (GameObject hole in holes)
        {
			hole.SetActive(false);
        }
        for (int i = 0; i < count; i++)
        {
			holes[i].gameObject.SetActive(true);
        }
    }

	public void RegisterScoreItem(PlayerScoreboardUI item)
	{
		playerScoreItems[item.Owner.Ref] = item;
	}

	public void SetRoundScores()
	{
		foreach (KeyValuePair<PlayerRef, PlayerScoreboardUI> kvp in playerScoreItems)
		{
			int hole = GameManager.Instance.CurrentHole;
			kvp.Value.SetScoreItem(hole, PlayerRegistry.GetPlayer(kvp.Key).Scores[hole]);
		}

		PlayerRegistry.OrderDesc(p => p.TotalScore)
			.Select(p => playerScoreItems.First(s => s.Key == p.Ref).Value)
			.ForEach((p, i) => p.SetPlacement(i + 1));
	}

	public void SumTotals()
	{
		foreach (var kvp in playerScoreItems)
		{
			kvp.Value.SetTotalScoreItem();
		}
	}

	public void Clear()
	{
		playerScoreItems.Clear();
		scoreHolder.DestroyChildren();
	}

	void PlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		if (playerScoreItems.TryGetValue(player, out PlayerScoreboardUI score))
		{
			GameManager.Instance.Runner.Despawn(score.Object);
			playerScoreItems.Remove(player);
		}
	}
}
