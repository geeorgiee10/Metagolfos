using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public NetworkObject playerObject;

    public void PlayerJoined(PlayerRef player)
    {
        // In a ClientServer topology, only the server can spawn players.
        if (Runner.Topology == Topologies.ClientServer)
        {
            if (Runner.CanSpawn)
            {
                Runner.Spawn(playerObject, inputAuthority: player);
            }
        }
        // In a shared topology, every player can spawn, however, we only want the local player to spawn their own player
        else if (Runner.LocalPlayer == player)
        {
            Runner.Spawn(playerObject, inputAuthority: player);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        bool canDespawn = (Runner.Topology == Topologies.ClientServer && Runner.IsServer) || 
            (Runner.Topology == Topologies.Shared && Runner.IsSharedModeMasterClient);

        if (canDespawn)
        {
            PlayerObject leavingPlayer = PlayerRegistry.GetPlayer(player);
            Runner.Despawn(leavingPlayer.Object);
        }
    }
}
