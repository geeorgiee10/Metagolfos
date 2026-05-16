using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffRouletteUI : MonoBehaviour
{
    public Image buffIcon;

    public Sprite[] buffSprites;

    [Header("Names")]
    public TextMeshProUGUI buffNameText;
    public string[] buffNames;

    public float duration = 2f;
    public float initialSpeed = 0.05f;
    public float slowdownAmount = 0.01f;

    bool rolling = false;

    public CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup.alpha = 0f;
    }

    public void PlayRoulette(int finalIndex)
    {
        if (!rolling)
            StartCoroutine(RouletteCoroutine(finalIndex));
    }

    IEnumerator RouletteCoroutine(int finalIndex)
    {
        rolling = true;

        canvasGroup.alpha = 1f;

        float timer = 0f;
        float currentSpeed = initialSpeed;

        while (timer < duration)
        {
            int randomIndex = Random.Range(0, buffSprites.Length);

            buffIcon.sprite = buffSprites[randomIndex];

            
            if (buffNameText != null && buffNames.Length > randomIndex)
                buffNameText.text = buffNames[randomIndex];

            yield return new WaitForSeconds(currentSpeed);

            timer += currentSpeed;

            currentSpeed += slowdownAmount;
        }

       
        buffIcon.sprite = buffSprites[finalIndex];

        
        if (buffNameText != null && buffNames.Length > finalIndex)
            buffNameText.text = buffNames[finalIndex];

        yield return new WaitForSeconds(1f);
        
        rolling = false;

        canvasGroup.alpha = 0f;

    }
}