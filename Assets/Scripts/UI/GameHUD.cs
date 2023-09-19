using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public void LeaveGame()
    {
        // host
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostGameManager.ShutDown();
        }
        // client
        else
        {
            ClientSignleton.Instance.ClientGameManager.Disconnect();
        }
    }    
}
