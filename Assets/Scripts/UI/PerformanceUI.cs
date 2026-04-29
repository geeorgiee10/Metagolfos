using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PerformanceUI : MonoBehaviour
{
    public UIScreen screen;
    public TMP_Text timeText, strokesText;

	private void OnEnable()
	{
		StartCoroutine(Animate());
	}

	public void SetStrokesText(int strokes, int placement)
    {
		if (strokes == PlayerObject.STROKES_DNF)
		{
			strokesText.text = $"DNF ({GetPlacement(placement)})";
		}
		else
		{
			strokesText.text = $"{strokes} Strokes ({GetPlacement(placement)})";
		}
    }

    public void SetTimesText(float time, int placement)
    {
		Debug.Log(time);
		if (time == PlayerObject.TIME_DNF)
		{
			timeText.text = $"DNF ({GetPlacement(placement)})";
		}
		else
		{
			timeText.text = $"{(int)(time / 60f):00}:{time % 60:00.00} ({GetPlacement(placement)})";
		}
    }

	string GetPlacement(int place)
	{
		switch (place)
		{
			case 1: return "1st";
			case 2: return "2nd";
			case 3: return "3rd";
			default: return $"{place}th";
		}
	}

	IEnumerator Animate()
	{
		float t = 0;
		while (t < 2.5f)
		{
			screen.Group.alpha = 3f - Mathf.Abs(3f * (1f - (2 * t) / 2.5f));
			t += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		
		screen.Back();
	}
}
