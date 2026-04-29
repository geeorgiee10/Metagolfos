using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityUtil
{
	public static void DestroyChildren(this Transform tf)
	{
		foreach(Transform child in tf)
		{
			Object.Destroy(child.gameObject);
		}
	}

	public static void DestroyChildrenWithComponent<T>(this Transform tf) where T : Component
	{
		foreach(Transform child in tf)
		{
			if (child.TryGetComponent(out T _))
				Object.Destroy(child.gameObject);
		}
	}

	public static bool Equals(this object obj, params object[] matches)
	{
		foreach (object m in matches)
		{
			if (obj.Equals(m)) return true;
		}
		return false;
	}
}
