using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreItem : MonoBehaviour
{
    public TMP_Text scoreText;
    
    public void SetScore(int score)
    {
        scoreText.text = $"{score}";
    }
}
