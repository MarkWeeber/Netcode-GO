using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
	[SerializeField] private Transform lobbyItemParent;
	[SerializeField] private LobbyItem lobbyItemPrefab;

	private bool isJoining = false;
	private bool isRefreshing = false;

    private void OnEnable()
    {
		RefreshLobbiesListAsync();
    }

    public async void RefreshLobbiesListAsync()
    {
        if (isRefreshing)
        {
			return;
        }
		else
		{
			isRefreshing = true;
		}

		try
		{
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter
                (
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"
                ),
                new QueryFilter
                (
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0"
                ),
            };
            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            foreach (Transform item in lobbyItemParent)
            {
                Destroy(item.gameObject);
            }
            foreach (Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Initialyze(this, lobby);
            }

        }
		catch (LobbyServiceException exception)
		{
			Debug.Log(exception);
		}

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
		if (isJoining)
		{
			return;
		}
		else
		{
            isJoining = true;
        }

        try
		{
			Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
			string joinCode = joiningLobby.Data["JoinCode"].Value;
            await ClientSignleton.Instance.ClientGameManager.StartClientAsync(joinCode);
        }
		catch (LobbyServiceException exception)
		{
			Debug.Log(exception);
		}
		
		isJoining = false;
    }
}
