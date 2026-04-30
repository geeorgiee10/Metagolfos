using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class RandomPerk : MonoBehaviour
{
    private String[] abilities = {"Under2Hits","Speed","Under5s"};
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            
            int randomIndex = Random.Range(0, abilities.Length);
            string randomAbility = abilities[randomIndex];
            switch (randomAbility)
            {
                case "Under2Hits":
                    //Activar buff
                    //other.gameObject.GetComponent<Player>().Under2Hits;
                    break;

                case "Speed":
                   //Activar buff
                   //other.gameObject.GetComponent<Player>().Speed;
                    break;

                case "Under5s":
                    //Activar buff
                    //other.gameObject.GetComponent<Player>().Under5s;
                    break;
                
            }
            Destroy(this.gameObject);
        }
    }
}
