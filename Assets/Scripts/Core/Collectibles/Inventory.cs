using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private BountyCoin bountyCoinPrefab;
    [SerializeField] private Health health;

    [Header("Settings")]
    [SerializeField] private int bountyCoinsCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] private float bountyCoinSpread = 3f;
    [Range(0f,100f)]
    [SerializeField] private float bountyPercentage = 50f;
    [SerializeField] private LayerMask layerMaskForBountyCoins;

    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();
    private Coin triggeredCoin;
    private int triggeredCoinValue;
    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;
    private int collisionsOnSpawnPoint;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        coinRadius = bountyCoinPrefab.GetComponent<CircleCollider2D>().radius;
        health.OnDie.AddListener(HandleOnDie);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {
            return;
        }
        health.OnDie.RemoveListener(HandleOnDie);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Utils.CheckLayer(targetMask, collision.gameObject.layer))
        {
            if (collision.TryGetComponent<Coin>(out triggeredCoin))
            {
                triggeredCoinValue = triggeredCoin.Collect();
                if (IsServer)
                {
                    TotalCoins.Value += triggeredCoinValue;
                }
            }
        }
    }

    private void HandleOnDie(Health health)
    {
        int bountyValue = (int)(TotalCoins.Value * (bountyPercentage / 100f));
        int bountyCoinValue = bountyValue / bountyCoinsCount;
        if (bountyCoinValue < minBountyCoinValue)
        {
            return;
        }
        for (int i = 0; i < bountyCoinsCount; i++)
        {
            BountyCoin bountyCoinInstance = Instantiate(bountyCoinPrefab, GetRandomSpawnPoint(), Quaternion.identity);
            bountyCoinInstance.SetValue(bountyCoinValue);
            bountyCoinInstance.NetworkObject.Spawn();
        }
    }

    private Vector2 GetRandomSpawnPoint()
    {
        int tries = 10;
        Vector2 spawnPoint = (Vector2) transform.position;
        while (tries > 0)
        {
            spawnPoint += UnityEngine.Random.insideUnitCircle * bountyCoinSpread;
            collisionsOnSpawnPoint = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMaskForBountyCoins);
            if (collisionsOnSpawnPoint == 0)
            {
                return spawnPoint;
            }
            tries--;
        }
        return spawnPoint;
    }
}
