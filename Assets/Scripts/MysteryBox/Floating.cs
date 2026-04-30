using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
   
    private float height = 0.1f;    
    private float speed = 1f;       

   
    private bool rotate = true;
    private float rotationSpeed = 30f;

    private Vector3 startPosition;

    void Start()
    {
        
        startPosition = transform.position;
    }

    void Update()
    {
        
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * height;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

       
        if (rotate)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }
}
