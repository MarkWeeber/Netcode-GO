using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private PlayerInstance playerPrefab;
    [Range(0f, 100f)]
    [SerializeField] private float keptCoinPercentage = 50f;

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
        int keptCoins = (int)(playerInstance.Inventory.TotalCoins.Value * (keptCoinPercentage / 100));
        Destroy(playerInstance.gameObject);
        StartCoroutine(RespawnPlayer(playerInstance.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
    {
        yield return new WaitForSeconds(1.5f);
        //yield return null; // wait until next frame
        PlayerInstance player = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPoint(), Quaternion.identity);
        player.NetworkObject.SpawnAsPlayerObject(ownerClientId);
        player.Inventory.TotalCoins.Value += keptCoins;
    }
}
