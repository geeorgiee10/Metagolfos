using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserData
{
	public static string Nickname
	{
		get
		{
			return PlayerPrefs.GetString("Nickname");
		}
		set
		{
			if (string.IsNullOrWhiteSpace(value))
				PlayerPrefs.DeleteKey("Nickname");
			else
				PlayerPrefs.SetString("Nickname", value);
		}
	}

	public static bool HasNickname => PlayerPrefs.HasKey("Nickname");
}
