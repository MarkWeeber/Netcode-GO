using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    private const int MaxConnections = 20;
    private const string MainMenuName = "MainMenu";
    private JoinAllocation joinAllocation;
    private UnityTransport unityTransport;
    private string joinCode;


    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();
        AuthenticationState authState = await AuthenticationWrapper.DoAuthenticate();
        if (authState == AuthenticationState.Authenticated)
        {
            return true;
        }
        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MainMenuName);
    }

    public async Task StartClientAsync(string passedJoinCode)
    {
        joinCode = passedJoinCode;
        try
        {
            joinAllocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }

        unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = new RelayServerData(
                                                                joinAllocation,
                                                                "dtls" // variants are 'udp' and 'dtls'
                                                                );
        unityTransport.SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();
    }
}
