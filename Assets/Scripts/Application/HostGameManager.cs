using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class HostGameManager
{
    private const int MaxConnections = 20;
    public const string GameSceneName = "Game";
    private Allocation allocation;
    private string joinCode;
    private UnityTransport unityTransport;

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

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
