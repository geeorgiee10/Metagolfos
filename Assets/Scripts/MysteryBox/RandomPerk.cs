using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class RandomPerk : MonoBehaviour
{
    //private String[] abilities = {"SuperHit","Intangible","Freeze","Teleport","SuperStar","MagneticField"};
    private String[] abilities = { "SuperHit", "Freeze" };
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
                    Debug.Log("SuperHit activated for: " + other.gameObject.GetComponent<Putter>().nombre);
                    superHit(other.gameObject);
                    break;

                case "Intangible":
                   //Activar buff
                    StartCoroutine(Intangible(other.gameObject));
                    break;

                case "Freeze":
                    //Activar buff
                    Debug.Log("Freeze activated for: " + other.gameObject.GetComponent<Putter>().nombre);
                    Freeze(other.gameObject);
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
            //Destroy(this.gameObject);
            gameObject.SetActive(false);
        }
    }
    private IEnumerator Freeze(GameObject gameObject)
    {
        foreach (PlayerObject player in PlayerRegistry.Players)
        {
            if (player.Nickname == gameObject.GetComponent<Putter>().nombre)
            {
                Debug.Log("if Player found: " + player.Nickname + ": " + gameObject.GetComponent<Putter>().nombre);
                continue;
            }
            Debug.Log("Player found: " + player.Nickname + ": " + gameObject.GetComponent<Putter>().nombre);
            player.Controller.freeze = true;
        }

        yield return new WaitForSeconds(5f);

        foreach (PlayerObject player in PlayerRegistry.Players)
        {
            player.Controller.freeze = false;
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

    private void superHit(GameObject gameObject)
    {
        gameObject.GetComponent<Putter>().maxPuttStrength = 20;
        Debug.Log("SuperHit: " + gameObject.name + "fuerza" + gameObject.GetComponent<Putter>().maxPuttStrength);
    }
}

