using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Text;
using Unity.Services.Authentication;

public class HostGameManager : IDisposable
{
    private const int MaxConnections = 20;
    public const string GameSceneName = "Game";
    private Allocation allocation;
    private string joinCode;
    private UnityTransport unityTransport;
    private Lobby lobby;
    private string lobbyId;
    public NetworkServer NetworkServer { get; private set; }

    public string JoinCode { get { return joinCode; } }

    public async Task StartHostAsync()
    {
		try
		{
            allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
        }
		catch (Exception exception)
		{
            Debug.LogError(exception);
            return;
		}
        try
        {
            joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        }
        catch (Exception exc)
        {
            Debug.LogError(exc);
            return;
        }
        unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(
                                                                allocation,
                                                                "dtls" // variants are 'udp' and 'dtls'
                                                                ); 
        unityTransport.SetRelayServerData(relayServerData);
        // creating lobby
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false; // private lobbies won't show on lobbies list
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject
                    (
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
                    )
                }
            };
            string playerName = PlayerPrefs.GetString(NameSelector.playerNameKey, "No name");
            lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15));
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log(exception);
            return;
        }

        // creating network server
        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        UserData userData = new UserData()
        {
            userName = PlayerPrefs.GetString(NameSelector.playerNameKey, "No name"),
            userAuthId = AuthenticationService.Instance.PlayerId,
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private IEnumerator HeartBeatLobby(float waitTimeSecconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSecconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public async void Dispose()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartBeatLobby));
        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException exception)
            {
                Debug.Log(exception);
            }
            lobbyId = string.Empty;
        }
        NetworkServer?.Dispose();
    }
}
