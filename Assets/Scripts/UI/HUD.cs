using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
	[SerializeField] TMP_Text timeText;
	[SerializeField] TMP_Text levelNameText, introLevelNameText;
	[SerializeField] Animator introAnimator;
	[SerializeField] TMP_Text strokesText;
	[Space]
	[SerializeField] Image puttChargeDot;
	[SerializeField] Image puttCharge;
	[SerializeField] Image puttCooldown;
	[SerializeField] CanvasGroup puttCooldownGroup;
	[field: SerializeField] public Gradient PuttChargeColor { get; private set; }
	[field: SerializeField, Space] public GameObject SpectatingObj { get; private set; }
	static IEnumerator puttChargeRoutine = null;
	static IEnumerator puttCooldownRoutine = null;

	public static HUD Instance { get; private set; }
	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		if (puttChargeRoutine != null)
			StartCoroutine(puttChargeRoutine);

		if (puttCooldownRoutine != null)
			StartCoroutine(puttCooldownRoutine);
	}

	public static void SetLevelName(int holeIndex)
	{
		Instance.introLevelNameText.text = Instance.levelNameText.text = $"Hole {holeIndex + 1}";
		Instance.introAnimator.SetTrigger("Intro");
	}

	public static void SetTimerText(float time)
	{
		Instance.timeText.text = $"{(int)(time / 60f):00}:{time % 60:00.00}";
	}

	public static void SetStrokeCount(int count)
	{
		Instance.strokesText.text = $"Strokes: {count}";
	}

	public static void SetPuttCharge(float fill, bool canPutt)
	{
		Instance.puttCharge.fillAmount = fill;
		Instance.puttCharge.color = canPutt ? Instance.PuttChargeColor.Evaluate(fill) : Color.gray;
	}

	public static void SetPuttCooldown(float fill)
	{
		Instance.puttCooldown.fillAmount = fill;
	}

	public static void ShowPuttCharge()
	{
		if (puttChargeRoutine != null)
		{
			Instance.StopCoroutine(puttChargeRoutine);
		}
		puttChargeRoutine = Instance.ShowPuttChargeRoutine();

		if (Instance.gameObject.activeInHierarchy)
		{
			Instance.StartCoroutine(puttChargeRoutine);
		}
	}

	IEnumerator ShowPuttChargeRoutine()
	{
		puttChargeDot.color = new Color(1, 1, 1, 0);
		puttCharge.rectTransform.localScale = Vector3.zero;

		float t = 0;
		while (t < 1)
		{
			puttCharge.rectTransform.localScale = Vector3.Lerp(puttCharge.rectTransform.localScale, Vector3.one, t);
			puttChargeDot.color = Color.Lerp(puttChargeDot.color, Color.white, t);
			t += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		puttCharge.rectTransform.localScale = Vector3.one;
		puttChargeDot.color = Color.white;
		puttChargeRoutine = null;
	}

	public static void HidePuttCharge()
	{
		if (puttChargeRoutine != null)
		{
			Instance.StopCoroutine(puttChargeRoutine);
		}
		puttChargeRoutine = Instance.HidePuttChargeRoutine();

		if (Instance.gameObject.activeInHierarchy)
		{
			Instance.StartCoroutine(puttChargeRoutine);
		}
	}

	IEnumerator HidePuttChargeRoutine()
	{
		puttChargeDot.color = Color.white;
		puttCharge.rectTransform.localScale = Vector3.one;

		Color clear = new Color(1, 1, 1, 0);
		float t = 0;
		while (t < 1)
		{
			puttCharge.fillAmount = Mathf.Lerp(puttCharge.fillAmount, 0, t);
			
			puttChargeDot.color = Color.Lerp(puttChargeDot.color, clear, t);
			t += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		puttCharge.fillAmount = 0;
		puttChargeRoutine = null;
	}

	public static void ShowPuttCooldown()
	{
		if (puttCooldownRoutine != null)
		{
			Instance.StopCoroutine(puttCooldownRoutine);
		}
		puttCooldownRoutine = Instance.ShowPuttCooldownRoutine();

		if (Instance.gameObject.activeInHierarchy)
		{
			Instance.StartCoroutine(puttCooldownRoutine);
		}
	}

	IEnumerator ShowPuttCooldownRoutine()
	{
		puttCooldownGroup.alpha = 0;
		puttCooldownGroup.transform.localScale = new Vector3(0.5f, 1, 1);
		float t = 0;
		while (t < 1)
		{
			puttCooldownGroup.alpha = Mathf.Lerp(puttCooldownGroup.alpha, 1, t);
			puttCooldownGroup.transform.localScale = Vector3.Lerp(puttCooldownGroup.transform.localScale, Vector3.one, t);
			t += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		puttCooldownGroup.alpha = 1;
		puttCooldownGroup.transform.localScale = Vector3.one;
		puttCooldownRoutine = null;
	}

	public static void HidePuttCooldown()
	{
		if (puttCooldownRoutine != null)
		{
			Instance.StopCoroutine(puttCooldownRoutine);
		}
		puttCooldownRoutine = Instance.HidePuttCooldownRoutine();

		if (Instance.gameObject.activeInHierarchy)
		{
			Instance.StartCoroutine(puttCooldownRoutine);
		}
	}

	IEnumerator HidePuttCooldownRoutine()
	{
		puttCooldownGroup.alpha = 1;
		puttCooldownGroup.transform.localScale = Vector3.one;
		float t = 0;
		while (t < 1)
		{
			puttCooldownGroup.alpha = Mathf.Lerp(puttCooldownGroup.alpha, 0, t);
			t += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		puttCooldownGroup.alpha = 0;
		puttCooldownRoutine = null;
	}

	public static void ForceHideAll()
	{
		puttChargeRoutine = null;
		puttCooldownRoutine = null;
		Instance.StopAllCoroutines();
		Instance.puttCharge.fillAmount = 0;
		Instance.puttCharge.rectTransform.localScale = Vector3.one;
		Instance.puttCooldownGroup.alpha = 0;
		Instance.puttCooldownGroup.transform.localScale = Vector3.one;
	}
}
