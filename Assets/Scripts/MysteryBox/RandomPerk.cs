using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class RandomPerk : MonoBehaviour
{
    private String[] abilities = {"SuperHit","Intangible","Freeze","Teleport","SuperStar","MagneticField"};
    [SerializeField] private Transform holePosition;
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            
            int randomIndex = Random.Range(0, abilities.Length);
            string randomAbility = abilities[randomIndex];
            switch (randomAbility)
            {
                case "SuperHit":
                    //Activar buff
                    
                    break;

                case "Intangible":
                   //Activar buff
                    StartCoroutine(Intangible(other.gameObject));
                    break;

                case "Freeze":
                    //Activar buff
                    //other.gameObject.GetComponent<Player>().Under5s;
                    break;
                case "Teleport":
                    //Activar buff
                    other.gameObject.transform.position = holePosition.position;
                    other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    break;
                case "SuperStar":
                    //Activar buff
                    StartCoroutine(SuperStar(other.gameObject));
                    break;
                case "MagneticField":
                    //Activar buff
                    //other.gameObject.GetComponent<Player>().Under5s;
                    break;
                
                
            }
            Destroy(this.gameObject);
        }
    }
    
    private IEnumerator Intangible(GameObject gameObject)
    {
        if (gameObject.GetComponent<SphereCollider>() != null)
        {
            gameObject.GetComponent<SphereCollider>().isTrigger = true;
            yield return new WaitForSeconds(8f);
            gameObject.GetComponent<SphereCollider>().isTrigger = false;
        }
    }

    private IEnumerator SuperStar(GameObject gameObject)
    {
        if (gameObject.GetComponent<MeshRenderer>() != null)
        {
            Color originalColor = gameObject.GetComponent<MeshRenderer>().material.color;
            int buffTime = 0;
            while (buffTime <= 8)
            {
                gameObject.GetComponent<MeshRenderer>().material.color = new Color(
                    Random.value,
                    Random.value,
                    Random.value
                );
                yield return new WaitForSeconds(1f);
                buffTime += 1;

            }

            gameObject.GetComponent<MeshRenderer>().material.color = originalColor;
            buffTime = 0;
        }
        yield return null;
    }
}

