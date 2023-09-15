using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        PlayerInstance[] playerInstances = FindObjectsOfType<PlayerInstance>();
        foreach (PlayerInstance playerInstance in playerInstances)
        {
            HandlePlayerSpawned(playerInstance);
        }
        PlayerInstance.OnPlayerSpawned.AddListener(HandlePlayerSpawned);
        PlayerInstance.OnPlayerDespawned.AddListener(HandlePlayerDespawned);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {
            return;
        }
        PlayerInstance.OnPlayerSpawned.RemoveListener(HandlePlayerSpawned);
        PlayerInstance.OnPlayerDespawned.RemoveListener(HandlePlayerDespawned);
    }

    private void HandlePlayerSpawned(PlayerInstance playerInstance)
    {
        playerInstance.Health.OnDie.AddListener((health) => HandlePlayerDie(playerInstance));
    }

    private void HandlePlayerDespawned(PlayerInstance playerInstance)
    {
        playerInstance.Health.OnDie.RemoveListener((health) => HandlePlayerDie(playerInstance));
    }

    private void HandlePlayerDie(PlayerInstance playerInstance)
    {
        Destroy(playerInstance.gameObject);
        StartCoroutine(RespawnPlayer(playerInstance.OwnerClientId));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId)
    {
        yield return new WaitForSeconds(2f);
        //yield return null; // wait until next frame
        NetworkObject networkObject = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPoint(), Quaternion.identity);
        networkObject.SpawnAsPlayerObject(ownerClientId);
    }
}
