using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WorldNickname : MonoBehaviour
{
	public TMP_Text worldNicknameText;
	public Vector3 offset;

	[HideInInspector] public Putter putter;
	[HideInInspector] public Transform target;

	public void SetTarget(Putter owner)
	{
		putter = owner;
		target = putter.interpolationTarget;
		worldNicknameText.text = putter.PlayerObj.Nickname;
	}

	private void LateUpdate()
	{
		if (target)
		{
			transform.position = target.position + offset;
			transform.rotation = CameraController.Instance.transform.rotation;
		}
		else
		{
			StartCoroutine(WaitAndDestroy());
		}
	}

	private IEnumerator WaitAndDestroy()
	{
		yield return new WaitForSeconds(1);
		if (target != null && !target.Equals(null))
		{
			// continue following the target
			yield return null;
		}
		else // there has been no target to follow for 1 second so Destroy this:
		{
			Destroy(gameObject);
		}
	}
}
