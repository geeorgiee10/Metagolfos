using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScrollingText : MonoBehaviour
{
    private Text text;

    public float xSpeed, ySpeed;
    private float xVal, yVal;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        xVal += Time.deltaTime * xSpeed;
        yVal += Time.deltaTime * ySpeed;
        text.rectTransform.position = new Vector3(text.rectTransform.position.x+xVal, text.rectTransform.position.y+yVal, text.rectTransform.position.z);
    }
}
