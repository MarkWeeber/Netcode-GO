using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoard : NetworkBehaviour
{
    [SerializeField] private Transform leaderBoardEntityHolder;
    [SerializeField] private LeaderBoardEntityDisplay leaderBoardEntityPrefab;
    [SerializeField] private int entitiesToDisplay = 8;
    private NetworkList<LeaderBoardEntityState> leaderBoardEntityStates;
    private List<LeaderBoardEntityDisplay> leaderBoardEntities = new List<LeaderBoardEntityDisplay>();

    private void Awake()
    {
        leaderBoardEntityStates = new NetworkList<LeaderBoardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            leaderBoardEntityStates.OnListChanged += HandleLeaderBoardEntitiesChanged;
            foreach (LeaderBoardEntityState item in leaderBoardEntityStates)
            {
                HandleLeaderBoardEntitiesChanged(new NetworkListEvent<LeaderBoardEntityState>
                {
                    Type = NetworkListEvent<LeaderBoardEntityState>.EventType.Add,
                    Value = item
                });
            }
        }
        if (IsServer)
        {
            HandleAlreadySpawnedPlayers();
        }    
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderBoardEntityStates.OnListChanged -= HandleLeaderBoardEntitiesChanged;
        }
        if (IsServer)
        {
            PlayerInstance.OnPlayerSpawned.RemoveListener(HandlePlayerSpawn);
            PlayerInstance.OnPlayerDespawned.RemoveListener(HandlePlayerDespawn);
        }
    }

    private void HandleAlreadySpawnedPlayers()
    {
        PlayerInstance[] playerInstances = GameObject.FindObjectsOfType<PlayerInstance>();
        foreach (PlayerInstance player in playerInstances)
        {
            HandlePlayerSpawn(player);
        }
        PlayerInstance.OnPlayerSpawned.AddListener(HandlePlayerSpawn);
        PlayerInstance.OnPlayerDespawned.AddListener(HandlePlayerDespawn);
    }


    private void HandlePlayerSpawn(PlayerInstance player)
    {
        leaderBoardEntityStates.Add(new LeaderBoardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });
        player.Inventory.TotalCoins.OnValueChanged += (oldCoins, newCoins) => HandleCoinsChange(player.OwnerClientId, newCoins);
    }

    private void HandlePlayerDespawn(PlayerInstance player)
    {
        if (leaderBoardEntityStates == null)
        {
            return;
        }
        foreach (LeaderBoardEntityState item in leaderBoardEntityStates)
        {
            if (item.ClientId != player.OwnerClientId)
            {
                continue;
            }
            Debug.Log($"ID: {item.ClientId}, Name: {item.PlayerName}");
            foreach (LeaderBoardEntityState x in leaderBoardEntityStates)
            {
                Debug.Log($"List ID: {x.ClientId}, List Name: {x.PlayerName}");
            }
            leaderBoardEntityStates.Remove(item);
            break;
        }
        player.Inventory.TotalCoins.OnValueChanged -= (oldCoins, newCoins) => HandleCoinsChange(player.OwnerClientId, newCoins);
    }

    private void HandleLeaderBoardEntitiesChanged(NetworkListEvent<LeaderBoardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Add:
                if (!leaderBoardEntities.Any(item => item.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderBoardEntityDisplay entityDisplay = Instantiate(leaderBoardEntityPrefab, leaderBoardEntityHolder);
                    entityDisplay.Initialize(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Coins);
                    leaderBoardEntities.Add(entityDisplay);
                }
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Remove:
                LeaderBoardEntityDisplay entityDisplayToBeRemoved = leaderBoardEntities.FirstOrDefault(item => item.ClientId == changeEvent.Value.ClientId);
                if (entityDisplayToBeRemoved != null)
                {
                    entityDisplayToBeRemoved.transform.SetParent(null);
                    Destroy(entityDisplayToBeRemoved.gameObject);
                    leaderBoardEntities.Remove(entityDisplayToBeRemoved);
                }
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Value:
                LeaderBoardEntityDisplay entityDisplayToBeUpdated = leaderBoardEntities.FirstOrDefault(item => item.ClientId == changeEvent.Value.ClientId);
                if (entityDisplayToBeUpdated != null)
                {
                    entityDisplayToBeUpdated.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
            default:
                break;
        }
        leaderBoardEntities.Sort((x, y) => y.Coins.CompareTo(x.Coins));
        for (int i = 0; i < leaderBoardEntities.Count; i++)
        {
            leaderBoardEntities[i].transform.SetSiblingIndex(i);
            leaderBoardEntities[i].UpdateText();
            bool shouldShow = i <= entitiesToDisplay - 1;
            leaderBoardEntities[i].gameObject.SetActive(shouldShow);
        }
        LeaderBoardEntityDisplay myDisplay = leaderBoardEntities.FirstOrDefault(item => item.ClientId == NetworkManager.Singleton.LocalClientId);
        if (myDisplay != null)
        {
            if (myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
                leaderBoardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }
    }

    private void HandleCoinsChange(ulong clientId, int newCoins)
    {
        for (int i = 0; i < leaderBoardEntityStates.Count; i++)
        {
            if (leaderBoardEntityStates[i].ClientId != clientId)
            {
                continue;
            }
            leaderBoardEntityStates[i] = new LeaderBoardEntityState
            {
                ClientId = leaderBoardEntityStates[i].ClientId,
                PlayerName = leaderBoardEntityStates[i].PlayerName,
                Coins = newCoins
            };
            return;
        }
    }
}

