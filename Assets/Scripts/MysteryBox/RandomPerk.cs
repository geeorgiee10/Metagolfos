using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class BuffData
{
    public string buffName;

    [Range(0,100)]
    public int probability;
}


public class RandomPerk : NetworkBehaviour
{
    [Header("Buffs")]
    public BuffData[] abilities;

    [Header("References")]
    [SerializeField] private Transform holePosition;

    private bool used = false;

    private void OnCollisionEnter(Collision other)
    {
        if (used) return;

        if (other.gameObject.CompareTag("Player"))
        {
            used = true;

            StartCoroutine(GiveRandomPerk(other.gameObject));
        }
    }
    

    private IEnumerator GiveRandomPerk(GameObject player)
    {
        // Elegir buff con probabilidades
        string randomAbility = GetRandomBuff();

        // Obtener índice para la UI
        int finalIndex = GetBuffIndex(randomAbility);

        // Mostrar ruleta
        if (player.GetComponent<NetworkObject>().HasInputAuthority)
        {
            InterfaceManager.Instance.buffRouletteUI.PlayRoulette(finalIndex);
        }

        // Esperar animación
        yield return new WaitForSeconds(2f);

        // Aplicar buff
        switch (randomAbility)
        {
            case "SuperHit":

                superHit(player);

                break;

            case "Intangible":

                if (player.GetComponent<Putter>() != null)
                    player.GetComponent<Putter>().startIntangible();

                break;

            case "Freeze":

                if (player.GetComponent<Putter>() != null)
                    player.GetComponent<Putter>().startFreeze();

                break;

            case "Teleport":

                Vector3 positionOffset = new Vector3(0, 1, 0);

                player.transform.position =
                    holePosition.position + positionOffset;

                player.GetComponent<Rigidbody>().velocity =
                    Vector3.zero;

                break;

            case "SuperStar":

                if (player.GetComponent<Putter>() != null)
                    player.GetComponent<Putter>().startSuperstar();

                break;

            case "MagneticField":

                //Activar buff
                //other.gameObject.GetComponent<Player>().Under5s;

                break;
        }

        // Destruir caja
        Runner.Despawn(Object);
    }

    private string GetRandomBuff()
    {
        int totalWeight = 0;

        foreach (BuffData buff in abilities)
        {
            totalWeight += buff.probability;
        }

        int randomNumber = Random.Range(0, totalWeight);

        int currentWeight = 0;

        foreach (BuffData buff in abilities)
        {
            currentWeight += buff.probability;

            if (randomNumber < currentWeight)
            {
                return buff.buffName;
            }
        }

        return abilities[0].buffName;
    }

    private int GetBuffIndex(string buffName)
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i].buffName == buffName)
            {
                return i;
            }
        }

        return 0;
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
        //Debug.Log("SuperHit: " + gameObject.name + "fuerza" + gameObject.GetComponent<Putter>().maxPuttStrength);
    }
}

