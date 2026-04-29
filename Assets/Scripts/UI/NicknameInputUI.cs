using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NicknameInputUI : MonoBehaviour
{
	public TMP_InputField nicknameField;

    private void OnEnable()
    {
        if (UserData.HasNickname)
        {
            nicknameField.text = UserData.Nickname;
        }
    }
    public void SaveNickname(string value)
    {
        UserData.Nickname = value;
    }
}
