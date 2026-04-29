using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Fusion;

public class PlayerScoreboardUI : NetworkBehaviour
{
    public TMP_Text usernameText, placementText;
    public Transform scoreHolder;
    public List<ScoreItem> scoreItems;
    public ScoreItem totalScoreItem;

	[Networked]
	public PlayerObject Owner { get; set; }
	[Networked, Capacity(18)]
	NetworkArray<byte> Scores { get; }

	public override void Spawned()
	{
		if (Owner)
		{
			Init(Owner);
		}
	}

	public void Init(PlayerObject pObj)
	{
		Owner = pObj;
		transform.SetParent(InterfaceManager.Instance.resultsScreen.scoreHolder, false);
		SpawnScoreItems(GameManager.CourseLength);
		usernameText.text = Owner.Nickname;
		InterfaceManager.Instance.resultsScreen.RegisterScoreItem(this);
	}

    void SpawnScoreItems(int holes)
    {
        for (int i = 0; i < holes; i++)
        {
            ScoreItem score = Instantiate(ResourcesManager.Instance.scoreItem, scoreHolder);
            scoreItems.Add(score);
        }
        totalScoreItem.transform.SetAsLastSibling();
    }

    public void SetScoreItem(int index, byte score)
    {
		Scores.Set(index, score);
        scoreItems[index].SetScore(score);
		SetTotalScoreItem();
        SetPlacement(score); // replace me, See CalculateScores() in Game Manager
    }

    public void SetTotalScoreItem()
    {
		int total = 0;
		foreach (byte s in Scores) total += s;
        totalScoreItem.SetScore(total);
    }

    public void SetPlacement(int num)
    {
		placementText.text = num == 1 ? "1st" : num == 2 ? "2nd" : num == 3 ? "3rd" : "";

		/*
        if (num <= 0) return num.ToString();

        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return num + "th";
        }

        switch (num % 10)
        {
            case 1:
                return num + "st";
            case 2:
                return num + "nd";
            case 3:
                return num + "rd";
            default:
                return num + "th";
        }
		//*/
    }
}
