using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;


public class RandomPerk : NetworkBehaviour
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
                    if(other.gameObject.GetComponent<Putter>()!=null)
                        other.gameObject.GetComponent<Putter>().startIntangible();
                    break;

                case "Freeze":
                    //Activar buff
                    //other.gameObject.GetComponent<Player>().Under5s;
                    break;
                case "Teleport":
                    //Activar buff
                    Vector3 positionOffset = new Vector3(0, 1, 0);
                    other.gameObject.transform.position = holePosition.position + positionOffset;
                    other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    break;
                case "SuperStar":
                    //Activar buff
                    if(other.gameObject.GetComponent<Putter>()!=null)
                        other.gameObject.GetComponent<Putter>().startSuperstar();
                    break;
                case "MagneticField":
                    //Activar buff
                    //other.gameObject.GetComponent<Player>().Under5s;
                    break;
                
                
            }
            Runner.Despawn(Object);
        }
    }
}

