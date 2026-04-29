using UnityEngine;

public static class RoomCode
{
	static readonly System.Random rnd = new System.Random();
	public static string Create(int length = 4)
	{
		char[] chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

		string str = "";
		for (int i = 0; i < length; i++)
		{
			str += chars[rnd.Next(0, chars.Length)];
		}
		return str;
	}
}