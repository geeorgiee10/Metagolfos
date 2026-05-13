using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;
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
        if (!HasStateAuthority) return;

        if (used) return;

        if (other.gameObject.CompareTag("Player"))
        {
            used = true;

            var playerNetObj = other.gameObject.GetComponent<NetworkObject>();
            if (playerNetObj == null) return;

            gameObject.GetComponent<BoxCollider>().enabled = false;

            StartCoroutine(GiveRandomPerk(other.gameObject, playerNetObj.InputAuthority));
        }
    }
    

    private IEnumerator GiveRandomPerk(GameObject player, PlayerRef targetPlayer)
    {
        string randomAbility = GetRandomBuff();

        int finalIndex = GetBuffIndex(randomAbility);

        // Mostrar ruleta SOLO al jugador que tocó la caja
        RPC_ShowRoulette(targetPlayer, finalIndex);

        // Esperar animación
        yield return new WaitForSeconds(2f);

        // Aplicar buff
        switch (randomAbility)
        {
            case "SuperHit":

                SuperHit(player);

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
        }

        Runner.Despawn(Object);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowRoulette(PlayerRef targetPlayer, int finalIndex)
    {
        if (Runner.LocalPlayer != targetPlayer) return;

        if (InterfaceManager.Instance != null &&
            InterfaceManager.Instance.buffRouletteUI != null)
        {
            InterfaceManager.Instance.buffRouletteUI.PlayRoulette(finalIndex);
        }
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

    private void SuperHit(GameObject gameObject)
    {
        gameObject.GetComponent<Putter>().maxPuttStrength = 20;
        gameObject.GetComponent<Putter>().HasSuperHit = true;
        //Debug.Log("SuperHit: " + gameObject.name + "fuerza" + gameObject.GetComponent<Putter>().maxPuttStrength);
    }
}

